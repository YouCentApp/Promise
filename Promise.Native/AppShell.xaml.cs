namespace Promise.Native;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(UpdateApp), typeof(UpdateApp));
	}
}
