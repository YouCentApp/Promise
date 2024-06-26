using Promise.Lib;
namespace Promise.Api;

public static class SignUp
{
    public static async Task<IResult> Run(HttpContext context)
    {
        try
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
                user.Login.Length < Policy.MinimumUsernameLength || user.Password.Length < Policy.MinimumPasswordLength)
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
                    Cents = Promo.InitialLimit
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
            return Results.Json(new ApiResponseUser
            {
                Success = true,
                Error = "",
                Id = dbUser.Id,
                Login = dbUser.Login
            });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            MainLogger.LogError("Error signing up user : " + ex);
            return Results.Json(new { success = false, error = "Server error..." });
        }
    }
}
