﻿using Microsoft.Extensions.Logging;

namespace Promise.Native;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("Nunito.ttf", "YouCentNunito");
			});

		builder.Services.AddMauiBlazorWebView();

		builder.Services.AddScoped<IMyEnvironment, MyEnvironment>();
		builder.Services.AddScoped<ISettings, Settings>();
		builder.Services.AddScoped<ISessionStorage, SessionStorage>();
		builder.Services.AddScoped<AppState>();
		builder.Services.AddScoped<INavigationManager, NativeNavigationManager>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
		Environment.SetEnvironmentVariable(MyEnvironment.MauiEnviroment, MyEnvironment.Dev);
#endif

#if RELEASE
		Environment.SetEnvironmentVariable(MyEnvironment.MauiEnviroment, MyEnvironment.Prod);
#endif

		Environment.SetEnvironmentVariable(MyEnvironment.MauiEnviroment, MyEnvironment.Prod);

		return builder.Build();
	}
}
