using Promise.Api;
using Promise.Lib;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add DbContext to the DI container
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PromiseDb>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();







// MinVerSup endpoint. It checks if the version of the APP is supported.
app.MapGet("/minversup", () =>
{
    return new { major = 2, minor = 0, build = 0 };
})
.WithOpenApi();







// SignIn endpoint
app.MapPost("/signin", async (HttpContext context) =>
{
    using var db = context.RequestServices.GetRequiredService<PromiseDb>();
    User? user = null;
    try
    {
        user = await context.Request.ReadFromJsonAsync<User>();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        MainLogger.LogError("Error reading user from signin request : " + ex);
        return Results.Json(new { success = false, error = "Server error..." });
    }
    if (user is null || user.Login is null || user.Password is null ||
        user.Login.Length < 1 || user.Password.Length < 1)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return Results.Json(new { success = false, error = "No data or wrong data provided" });
    }
    var dbUser = db.Users.FirstOrDefault(u => u.Login == user.Login);
    if (dbUser is null || dbUser.Password is null || dbUser.Salt is null)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return Results.Json(new { success = false, error = "User not found" });
    }
    var hash = Security.GetPasswordHash(user.Password, dbUser.Salt);
    if (hash != dbUser.Password)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Results.Json(new { auth = false, error = "Wrong password!" });
    }
    var secret = configuration["Jwt:Secret"];
    if (secret is null)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        MainLogger.LogError("No secret provided for JWT");
        return Results.Json(new { success = false, error = "Server error..." });
    }
    var payload = new Dictionary<string, object>
    {
        { Security.PayLoadFieldLogin, user.Login },
        { Security.PayLoadFieldAuth, true },
        { Security.PayLoadFieldExp, Security.GetAccessTokenLifetimeSeconds() }
    };
    var token = Security.CreateBearerJwt(payload, secret);
    context.Response.Headers.TryAdd(Security.AuthorizationHttpHeader, token);
    context.Response.StatusCode = StatusCodes.Status202Accepted;
    return Results.Json(new
    {
        success = true,
        error = "",
        id = dbUser.Id,
        login = dbUser.Login
    });
})
.Accepts<User>("application/json", "User Login")
.WithOpenApi();










// SignUp endpoint
app.MapPost("/signup", async (HttpContext context) =>
{
    using var db = context.RequestServices.GetRequiredService<PromiseDb>();
    User? user = null;
    try
    {
        user = await context.Request.ReadFromJsonAsync<User>();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        MainLogger.LogError("Error reading user from signup request : " + ex);
        return Results.Json(new { success = false, error = "Server error..." });
    }
    if (user is null || user.Login is null || user.Password is null ||
        user.Login.Length < 5 || user.Password.Length < 8)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return Results.Json(new { success = false, error = "No data or wrong data provided" });
    }
    var dbUser = db.Users.FirstOrDefault(u => u.Login == user.Login);
    if (dbUser != null)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        return Results.Json(new { success = false, error = "Username already exists, sorry..." });
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
    var records = await db.SaveChangesAsync();
    dbUser = db.Users.FirstOrDefault(u => u.Login == user.Login);
    if (records > 0 && dbUser != null)
    {
        var balance = new Balance
        {
            UserId = newUser.Id,
            Cents = 0
        };
        db.Balances.Add(balance);
        var limit = new PromiseLimit
        {
            UserId = newUser.Id,
            Cents = 1500
        };
        db.PromiseLimits.Add(limit);
        var settings = new UserSetting
        {
            UserId = newUser.Id,
            CurrencyId = 1,
            LanguageId = 1,
            IsDarkTheme = false
        };
        db.UserSettings.Add(settings);
        records = await db.SaveChangesAsync();
        if (records < 3)
        {
            db.Users.Remove(dbUser);
            db.Balances.Remove(balance);
            db.PromiseLimits.Remove(limit);
            db.UserSettings.Remove(settings);
            await db.SaveChangesAsync();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            MainLogger.LogError("Error saving user related data to the DB");
            return Results.Json(new { success = false, error = "Server error..." });
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        MainLogger.LogError("Error saving user to DB");
        return Results.Json(new { success = false, error = "Server error..." });
    }
    context.Response.StatusCode = StatusCodes.Status202Accepted;
    return Results.Json(new
    {
        success = true,
        error = "",
        id = dbUser.Id,
        login = dbUser.Login
    });
})
.Accepts<User>("application/json", "User Registration")
.WithOpenApi();











// UserInfo endpoint
app.MapPost("/userinfo", async (HttpContext context) =>
{
    using var db = context.RequestServices.GetRequiredService<PromiseDb>();
    var jwt = context.Request.Headers[Security.AuthorizationHttpHeader].ToString();
    if (jwt.Length < 1)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Results.Json(new { success = false, error = "No token provided" });
    }

    User? user = null;
    try
    {
        user = await context.Request.ReadFromJsonAsync<User>();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        MainLogger.LogError("Error reading user from signin request : " + ex);
        return Results.Json(new { success = false, error = "Server error..." });
    }
    if (user is null || user.Login is null || user.Password is null ||
        user.Login.Length < 1 || user.Password.Length < 1)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return Results.Json(new { success = false, error = "No data or wrong data provided" });
    }

    var secret = configuration["Jwt:Secret"];
    if (secret is null)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        MainLogger.LogError("No secret provided for JWT");
        return Results.Json(new { success = false, error = "Server error..." });
    }
    if (!Security.ValidateBearerAccessToken(jwt, user.Login, secret))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Results.Json(new { success = false, error = "Invalid token" });
    }

    var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
    if (dbUser is null)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return Results.Json(new { success = false, error = "User not found" });
    }
    var balance = await db.Balances.FirstOrDefaultAsync(b => b.UserId == dbUser.Id);
    if (balance is null)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return Results.Json(new { success = false, error = "Balance not found" });
    }
    var limit = await db.PromiseLimits.FirstOrDefaultAsync(l => l.UserId == dbUser.Id);
    if (limit is null)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return Results.Json(new { success = false, error = "Promise limit not found" });
    }
    context.Response.StatusCode = StatusCodes.Status200OK;
    return Results.Json(new
    {
        success = true,
        error = "",
        id = dbUser.Id,
        login = dbUser.Login,
        balance = balance.Cents,
        limit = limit.Cents
    });
})
.Accepts<User>("application/json", "User Info")
.WithOpenApi();










// DataUpdate endpoint

app.MapPost("/dataupdate", async (HttpContext context) =>
{
    using var db = context.RequestServices.GetRequiredService<PromiseDb>();
    User? user = null;
    try
    {
        user = await context.Request.ReadFromJsonAsync<User>();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        MainLogger.LogError("Error reading user from personal data update request : " + ex);
        return Results.Json(new { success = false, error = "Server error..." });
    }
    if (user is null || user.Id < 1 || user.Login is null || user.Password is null ||
        user.Login.Length < 1 || user.Password.Length < 1)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return Results.Json(new { success = false, error = "No data or wrong data provided" });
    }

    var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
    if (dbUser is null || dbUser.Password is null || dbUser.Salt is null || dbUser.Id != user.Id)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return Results.Json(new { success = false, error = "User not found or login and Id don't match" });
    }
    var hash = Security.GetPasswordHash(user.Password, dbUser.Salt);
    if (hash != dbUser.Password)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Results.Json(new { auth = false, error = "Wrong password!" });
    }

    PersonalData? personalData = null;
    try
    {
        personalData = await context.Request.ReadFromJsonAsync<PersonalData>();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        MainLogger.LogError("Error reading personal data from personal data update request : " + ex);
        return Results.Json(new { success = false, error = "Server error..." });
    }
    if (personalData is null || personalData.Email is null || personalData.Tel is null || personalData.Secret is null)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return Results.Json(new { success = false, error = "No data or wrong data provided" });
    }

    var dbPersonalData = await db.PersonalData.FirstOrDefaultAsync(pd => pd.UserId == dbUser.Id);
    if (dbPersonalData is null)
    {
        dbPersonalData = new PersonalData
        {
            UserId = dbUser.Id,
            Email = "",
            Tel = "",
            Secret = "",
            EmailHash = "",
            TelHash = "",
            SecretHash = "",
            Salt = "",
            EmailMasked = "",
            TelMasked = ""
        };
        db.PersonalData.Add(dbPersonalData);
    }
    if (personalData.Email.Length > 0)
    {
        dbPersonalData.Email = personalData.Email;
    }
    if (personalData.Tel.Length > 0)
    {
        dbPersonalData.Tel = personalData.Tel;
    }
    if (personalData.Secret.Length > 0)
    {
        dbPersonalData.Secret = personalData.Secret;
    }
    var records = await db.SaveChangesAsync();
    if (records < 1)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        MainLogger.LogError("Error saving personal data to DB");
        return Results.Json(new { success = false, error = "Server error..." });
    }
    context.Response.StatusCode = StatusCodes.Status202Accepted;
    return Results.Json(new
    {
        success = true,
        error = ""
    });
})
.Accepts<User>("application/json", "User Info")
.WithOpenApi();


















// RUN!

app.Run();

