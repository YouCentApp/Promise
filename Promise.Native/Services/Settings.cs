public class Settings(IMyEnvironment myEnvironment) : ISettings
{
    private readonly IMyEnvironment myEnv = myEnvironment;

    public string ApiUrl
    {
        get
        {
            if (myEnv.IsProduction())
            {
                return Api.UrlProd;
            }
            if (myEnv.IsDevelopment())
            {
                return Api.UrlDev;
            }
            return string.Empty;
        }
    }
}
