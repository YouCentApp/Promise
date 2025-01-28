using Microsoft.EntityFrameworkCore;
using Promise.Lib;
using System.Security.Cryptography;
using System.Text;

namespace Promise.Api;

public static class RestoreAccessUseSecret
{
    private static string errorReason = string.Empty;
    public static async Task<IResult> Run(HttpContext context)
    {
        try
        {
            using var db = context.RequestServices.GetRequiredService<PromiseDb>();
            var request = await context.Request.ReadFromJsonAsync<RestoreAccessInfo>();

            if (request is null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.UseData))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return Results.Json(new { success = false, error = "Username and secret word are required." });
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Login == request.Username);
            if (user is null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Results.Json(new { success = false, error = "User not found." });
            }

            var personalData = await db.PersonalData.FirstOrDefaultAsync(pd => pd.UserId == user.Id);
            if (personalData is null || string.IsNullOrEmpty(personalData.SecretHash))
            {
                return Results.Json(new { success = false, error = "Sorry, you did not save a secret word before." });
            }

            var secretHash = Security.GetHash(request.UseData + personalData.Salt);
            if (secretHash != personalData.SecretHash)
            {
                await TrackFailedAttempt(db, user.Id);
                return Results.Json(new { success = false, error = "Incorrect secret word." });
            }

            var newPassword = GenerateTemporaryPassword();
            var newPasswordHash = Security.GetPasswordHash(newPassword, user.Salt!);
            user.Password = newPasswordHash;

            db.Users.Update(user);
            await db.SaveChangesAsync();

            return Results.Json(new { success = true, newPassword });
        }
        catch (Exception ex)
        {
            MainLogger.LogError($"An error occurred while restoring access. Exception: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return Results.Json(new { success = false, error = "Something went wrong... " + errorReason });
        }
    }

    private static async Task TrackFailedAttempt(PromiseDb db, long userId)
    {
        var attempt = new AccessRestore
        {
            UserId = userId,
            UseSecretTryDate = DateTime.UtcNow,
            UseSecretTryNumber = 1
        };
        db.Set<AccessRestore>().Add(attempt);
        await db.SaveChangesAsync();

        var attempts = await db.Set<AccessRestore>().Where(ar => ar.UserId == userId).ToListAsync();
        if (attempts.Count >= 3)
        {
            errorReason = "You have reached the maximum number of attempts. Please contact us or use other way to restore access.";
            throw new InvalidOperationException("Maximum number of attempts reached for user ID: " + userId);
        }
    }


    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}


