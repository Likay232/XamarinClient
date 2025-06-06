﻿namespace MauiApp;

using Microsoft.Maui.Hosting;
using Microsoft.Extensions.Logging;
using ApplicationExtension;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .RegisterSerivces()
            .RegisterViewModels()
            .RegisterViews();
        
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}