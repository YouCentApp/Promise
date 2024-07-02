using Promise.Api;
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
var mailSettings = configuration.GetSection(nameof(MailSettings));
builder.Services.Configure<MailSettings>(mailSettings);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

string? jwtSecret = configuration["Jwt:Secret"];


// MinVerSup endpoint. It checks if the version of the APP is supported.
app.MapGet("/minversup", () =>
{
    return new { major = 2, minor = 0, build = 0 };
})
.WithOpenApi();


// SignIn endpoint
app.MapPost("/signin", async (HttpContext context) =>
{
    //var secret = configuration["Jwt:Secret"];
#pragma warning disable CS0612 // Type or member is obsolete
    return await SignIn.Run(context, jwtSecret);
#pragma warning restore CS0612 // Type or member is obsolete
})
.Accepts<User>("application/json", "User data for Sign In")
.WithOpenApi();


// SignUp endpoint
app.MapPost("/signup", async (HttpContext context) =>
{
    return await SignUp.Run(context);
})
.Accepts<User>("application/json", "User data for Sign Up")
.WithOpenApi();


// UserInfo endpoint
app.MapPost("/userinfo", async (HttpContext context) =>
{
    //var secret = configuration["Jwt:Secret"];
#pragma warning disable CS0612 // Type or member is obsolete
    return await UserInfo.Run(context, jwtSecret);
#pragma warning restore CS0612 // Type or member is obsolete
})
.Accepts<User>("application/json", "User data for User Info")
.WithOpenApi();


// DataUpdate endpoint
app.MapPost("/dataupdate", async (HttpContext context) =>
{
    return await DataUpdate.Run(context);
})
.Accepts<UserData>("application/json", "User data for Data Update")
.WithOpenApi();


// DeleteUser endpoint
app.MapDelete("/deleteuser", async (HttpContext context) =>
{
#pragma warning disable CS0612 // Type or member is obsolete
    return await DeleteUser.Run(context, jwtSecret);
#pragma warning restore CS0612 // Type or member is obsolete
})
.Accepts<User>("application/json", "User data for Delete User")
.WithOpenApi();


// UpdatePassword endpoint
app.MapPut("/updatepassword", async (HttpContext context) =>
{
#pragma warning disable CS0612 // Type or member is obsolete
    return await UpdatePassword.Run(context, jwtSecret);
#pragma warning restore CS0612 // Type or member is obsolete
})
.Accepts<UserUpdate>("application/json", "User data for Update Password")
.WithOpenApi();


// SendPromises endpoint
app.MapPost("/sendpromises", async (HttpContext context) =>
{
#pragma warning disable CS0612 // Type or member is obsolete
    return await SendPromises.Run(context, jwtSecret);
#pragma warning restore CS0612 // Type or member is obsolete
})
.Accepts<UserTransaction>("application/json", "User data for Send Promises")
.WithOpenApi();




// RUN!

app.Run();

