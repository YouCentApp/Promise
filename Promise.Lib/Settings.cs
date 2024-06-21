using System.Environment;
public class Settings
{
    private const string apiUrlDev = "https://localhost:5014";
    private const string apiUrlProd = "https://api.youcent.app";
    private const string prod = "Production";
    private const string dev = "Development";
    private static readonly string _env = GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    public static string ApiUrl()
    {
        if (_env == prod)
        {
            return apiUrlProd;
        }
        if (_env == dev)
        {
            return apiUrlDev;
        }
        return string.Empty;
    }
}