using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Promise.Lib;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace Promise.Api;

public static class RestoreAccessUseEmail
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
                return Results.Json(new { success = false, error = "Username and email are required." });
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Login == request.Username);
            if (user is null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Results.Json(new { success = false, error = "User not found." });
            }

            var personalData = await db.PersonalData.FirstOrDefaultAsync(pd => pd.UserId == user.Id);
            if (personalData is null || string.IsNullOrEmpty(personalData.EmailHash))
            {
                return Results.Json(new { success = false, error = "Sorry, you did not save an email address before." });
            }

            var emailHash = Security.GetHash(request.UseData + personalData.Salt);
            if (emailHash != personalData.EmailHash)
            {
                await TrackFailedAttempt(db, user.Id);
                return Results.Json(new { 
                    success = false, 
                    error = "Incorrect email address.", 
                    maskedEmail = personalData.EmailMasked 
                });
            }

            var newPassword = Security.GenerateTemporaryPassword();
            var newPasswordHash = Security.GetPasswordHash(newPassword, user.Salt!);
            user.Password = newPasswordHash;

            var accessRestore = await db.Set<AccessRestore>().FirstOrDefaultAsync(ar => ar.UserId == user.Id);
            if (accessRestore != null)
            {
                accessRestore.UseSecretTryNumber = 0;
                accessRestore.UseEmailTryNumber = 0;
                accessRestore.UseTelTryNumber = 0;
                accessRestore.UseSecretTryDate = null;
                accessRestore.UseEmailTryDate = null;
                accessRestore.UseTelTryDate = null;
                db.Set<AccessRestore>().Update(accessRestore);
            }

            db.Users.Update(user);
            await db.SaveChangesAsync();

            var mailSettings = context.RequestServices.GetRequiredService<IOptions<MailSettings>>();
            var mailSender = new MailSender(mailSettings);

            var userMailData = new MailData(
                new List<string> { personalData.Email! },
                "Your Temporary YouCent Password",
                $"Your new temporary password for YouCent App is: {newPassword} <br><br> Please <b>change</b> it after signing in. <br>www.youcent.app"
            );

            var adminMailData = new MailData(
                new List<string> { MailSender.SystemEMail },
                "Temporary YouCent Password Sent to the User",
                $"A new temporary password was sent to {personalData.Email} for username: {user.Login}. <br>The password is: {newPassword}"
            );

            await mailSender.SendAsync(userMailData);
            await mailSender.SendAsync(adminMailData);

            return Results.Json(new { success = true });
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
        var accessRestore = await db.Set<AccessRestore>().FirstOrDefaultAsync(ar => ar.UserId == userId);
        if (accessRestore == null)
        {
            accessRestore = new AccessRestore
            {
                UserId = userId,
                UseEmailTryDate = DateTime.UtcNow,
                UseEmailTryNumber = 1
            };
            db.Set<AccessRestore>().Add(accessRestore);
        }
        else
        {
            accessRestore.UseEmailTryNumber++;
            accessRestore.UseEmailTryDate = DateTime.UtcNow;
            db.Set<AccessRestore>().Update(accessRestore);
        }
        await db.SaveChangesAsync();

        if (accessRestore.UseEmailTryNumber >= 3)
        {
            errorReason = "You have reached the maximum number of attempts. Please contact us or use another way to restore access.";
            throw new InvalidOperationException("Maximum number of attempts reached for user ID: " + userId);
        }
    }

}

