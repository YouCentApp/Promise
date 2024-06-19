using Microsoft.EntityFrameworkCore;
using Promise.Api;
using Promise.Lib;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext to the DI container
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<YCDBContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();




// Check if the version of the APP is supported
app.MapGet("/minversup", () =>
{
    return new { major = 2, minor = 0, build = 0 };
})
.WithOpenApi();



// Signin endpoint
app.MapPost("/signin", async (HttpContext context) =>
{
    using var db = context.RequestServices.GetRequiredService<YCDBContext>();
    User? user = null;
    try
    {
        user = await context.Request.ReadFromJsonAsync<User>();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        MainLogger.LogError("Error reading user from signin request : " + ex);
        return Results.Json(new { auth = false, error = "Server error..." });
    }
    if (user is null || user.Login is null || user.Password is null ||
        user.Login.Length < 1 || user.Password.Length < 1)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return Results.Json(new { auth = false, error = "No data or wrong data provided" });
    }
    var dbUser = db.Users.FirstOrDefault(u => u.Login == user.Login);
    if (dbUser is null)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return Results.Json(new { auth = false, error = "User not found" });
    }
    var hash = Security.GetHash(Security.GetHash(user.Password) + dbUser.Salt);
    if (hash != dbUser.Password) return Results.Json(new { auth = false, error = "Wrong password!" });

    var payload = new Dictionary<string, object>
    {
        { Security.PayLoadFieldLogin, user.Login },
        { Security.PayLoadFieldAuth, true },
        { Security.PayLoadFieldExp, DateTime.Now.AddHours(Security.AccessTokenLifetimeHours) }
    };
    var secret = configuration["Jwt:Secret"];
    var token = Security.CreateBearerJwt(payload, secret);
    context.Response.Headers.TryAdd(Security.AuthorizationHttpHeader, token);
    context.Response.StatusCode = StatusCodes.Status202Accepted;
    return Results.Json(new { auth = true, error = "" }); ;
})
.Accepts<User>("application/json", "User Login")
.WithOpenApi();






// Signup endpoint
app.MapPost("/signup", async (HttpContext context) =>
{
    using var db = context.RequestServices.GetRequiredService<YCDBContext>();
    User? user = null;
    try
    {
        user = await context.Request.ReadFromJsonAsync<User>();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        MainLogger.LogError("Error reading user from signup request : " + ex);
        return Results.Json(new { auth = false, error = "Server error..." });
    }
    if (user is null || user.Login is null || user.Password is null ||
        user.Login.Length < 5 || user.Password.Length < 8)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return Results.Json(new { auth = false, error = "No data or wrong data provided" });
    }
    var dbUser = db.Users.FirstOrDefault(u => u.Login == user.Login);
    if (dbUser != null)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        return Results.Json(new { auth = false, error = "Username already exists, sorry..." });
    }
    var salt = Security.GetSalt();
    var newUser = new User
    {
        Login = user.Login,
        Password = Security.GetPasswordHash(user.Password, salt),
        Salt = salt,
        CreationDate = DateTime.Now
    };
    db.Users.Add(newUser);
    await db.SaveChangesAsync();
    context.Response.StatusCode = StatusCodes.Status202Accepted;
    return Results.Json(new { auth = true, error = "" });
})
.Accepts<User>("application/json", "User Registration")
.WithOpenApi();


app.Run();

