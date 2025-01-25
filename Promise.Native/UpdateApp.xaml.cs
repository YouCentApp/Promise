namespace Promise.Native;

public partial class UpdateApp : ContentPage
{
	public UpdateApp()
	{
		InitializeComponent();
	}

	private async void UpdateForAndroid(object sender, EventArgs e)
	{
		await Launcher.OpenAsync(new Uri("https://play.google.com/store/apps/details?id=app.YouCent"));

	}

	private async void UpdateForiOS(object sender, EventArgs e)
	{
		await Launcher.OpenAsync(new Uri("https://apps.apple.com/us/app/youcent-pay-without-money/id1620023350"));
	}

	private async void UpdateForWindows(object sender, EventArgs e)
	{
		await Launcher.OpenAsync(new Uri("https://apps.microsoft.com/detail/9p40rw7nmzd4"));
	}

	//TODO: Add the link for MacOS later when its version up and running (for now just using iOS link)
	private async void UpdateForMacOS(object sender, EventArgs e)
	{
		await Launcher.OpenAsync(new Uri("https://apps.apple.com/us/app/youcent-pay-without-money/id1620023350"));
	}

	private async void UpdateForWeb(object sender, EventArgs e)
	{
		await Launcher.OpenAsync(new Uri("https://promisesite.azurewebsites.net/"));
	}

	// VisitWebsite
	private async void VisitWebsite(object sender, EventArgs e)
	{
		await Launcher.OpenAsync(new Uri("https://youcent.app"));
	}


}