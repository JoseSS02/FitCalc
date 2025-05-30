using FitCalc.Services;
using Microsoft.AspNetCore.Components.WebView.Maui;
// using FitCalc.Data; // Se usará después
// using FitCalc.Services; // Se usará después

namespace FitCalc;

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
            });

        // Blazor WebView (permite usar componentes Razor)
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddScoped<DatabaseService>();


#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        // Aquí podrías agregar tus servicios en el futuro
        // builder.Services.AddSingleton<UserService>();
        // builder.Services.AddDbContext<AppDbContext>();

        return builder.Build();
    }
}
