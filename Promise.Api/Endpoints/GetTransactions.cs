namespace Promise.Api;

public class GetTransactions
{
    [Obsolete]
    public static async Task<IResult> Run(HttpContext context, string? jwtSecret)
    {
        try
        {
            if (jwtSecret is null)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                MainLogger.LogError("No secret provided for JWT");
                return Results.Json(new { success = false, error = "Server error... Please try again later." });
            }
            var transactionsHistory = await context.Request.ReadFromJsonAsync<TransactionsHistory>();
            if (transactionsHistory is null || transactionsHistory.User is null || transactionsHistory.User.Login is null)
            {
                MainLogger.LogError("Error reading transactions history from get transactions request");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return Results.Json(new { success = false, error = "No data or wrong data provided." });
            }
            var jwt = context.Request.Headers[Security.AuthorizationHttpHeader].ToString();
            if (jwt is null || jwt.Length < 1)
            {
                MainLogger.LogError("No JWT provided for get transactions request for user with username: " + transactionsHistory.User.Login);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Results.Json(new { success = false, error = "Looks like you are not signed in, sorry." });
            }

            using var db = context.RequestServices.GetRequiredService<PromiseDb>();
            var user = db.Users.FirstOrDefault(u => u.Login == transactionsHistory.User.Login);
            if (user is null || user.Login is null)
            {
                MainLogger.LogError("User not found for get transactions request for username: " + transactionsHistory.User.Login);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Results.Json(new { success = false, error = "Something went wrong. Please try again later." });
            }

            if (!Security.ValidateBearerAccessToken(jwt, user.Login, jwtSecret))
            {
                MainLogger.LogError("Invalid JWT provided for get transactions request for username: " + user.Login);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Results.Json(new { success = false, error = "Your session is expired or invalid, please sign in again." });
            }

            var transactions = db.PromiseTransactions
                .Where(t => t.SenderId == user.Id || t.ReceiverId == user.Id)
                .Where(t => t.Date >= transactionsHistory.From && t.Date <= transactionsHistory.To);

            if (transactionsHistory.IsOldFirst)
            {
                transactions = transactions.OrderBy(t => t.Date);
            }
            else
            {
                transactions = transactions.OrderByDescending(t => t.Date);
            }

            var retrievedTransactions = transactions
            .Skip(transactionsHistory.Offset)
            .Take(transactionsHistory.Limit).ToList();

            return Results.Json(new ApiResponseUserTransactions
            {
                Success = true,
                Error = "",
                Id = user.Id,
                Login = user.Login,
                Transactions = retrievedTransactions
            });

        }
        catch (Exception ex)
        {
            MainLogger.LogError("Error in get transactions endpoint: " + ex.Message);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return Results.Json(new { success = false, error = "Server error... Please try again later." });
        }
    }

}
