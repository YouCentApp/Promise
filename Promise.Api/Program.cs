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





app.MapGet("/minversup", () =>
{
    return new { major = 2, minor = 0, build = 0 };
})
.WithOpenApi();




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
    if (user is null)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return Results.Json(new { auth = false, error = "No data provided" });
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
        { Security.PayLoadFieldExp, DateTime.Now.AddHours(1) }
    };
    var token = Security.CreateBearerJwt(payload, "!!!!!!!!!secret!!!!!!!!!!!!!!");
    context.Response.Headers.TryAdd(Security.AuthorizationHttpHeader, token);
    return Results.Ok();
})
.Accepts<User>("application/json", "User Login")
.WithOpenApi();




app.Run();

