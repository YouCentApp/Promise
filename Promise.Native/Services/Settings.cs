
public class Settings(IMyEnvironment myEnvironment) : ISettings
{
    private readonly IMyEnvironment myEnv = myEnvironment;
    private bool IsAndroid() => DeviceInfo.Current.Platform == DevicePlatform.Android;
    private bool IsiOS() => DeviceInfo.Current.Platform == DevicePlatform.iOS;
    private bool IsmacOS() => DeviceInfo.Current.Platform == DevicePlatform.macOS;
    private bool IsMacCatalyst() => DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst;
    private bool IsWinUI() => DeviceInfo.Current.Platform == DevicePlatform.WinUI;

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
                if (IsAndroid())
                {
                    return Api.UrlDevAndroid;
                }
                return Api.UrlDev;
            }
            return string.Empty;
        }
    }
}
