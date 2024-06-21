using System;
public class Settings
{
    private const string apiUrlDev = "https://localhost:5014";
    private const string apiUrlProd = "https://api.youcent.app";
    private const string prod = "Production";
    private const string dev = "Development";
    private const string env = "ASPNETCORE_ENVIRONMENT";

    public static string GetApiUrl()
    {
        if (Environment.GetEnvironmentVariable(env) == prod)
        {
            return apiUrlProd;
        }
        if (Environment.GetEnvironmentVariable(env) == dev)
        {
            return apiUrlDev;
        }
        return string.Empty;
    }
}