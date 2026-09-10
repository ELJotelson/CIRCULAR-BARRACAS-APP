using Circulacion_Barracas.Pages;
using Circulacion_Barracas.Services;
using Microsoft.Extensions.Logging;

namespace Circulacion_Barracas
{
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
                });

            builder.Services.AddSingleton<SupabaseService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<OperacionService>();
            builder.Services.AddSingleton<UpdateService>();
            builder.Services.AddSingleton(_ =>
                new DatabaseService(Path.Combine(FileSystem.AppDataDirectory, "desvios.db3")));

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SignUpPage>();
            builder.Services.AddTransient<OperacionPage>();
            builder.Services.AddTransient<NuevoDesvioPage>();
            builder.Services.AddTransient<ListaDesviosPage>();
            builder.Services.AddTransient<NominaPage>();
            builder.Services.AddTransient<AlertasPage>();
            builder.Services.AddTransient<CatalogosPage>();
            builder.Services.AddTransient<MotivosDesvioPage>();
            builder.Services.AddTransient<UsuariosPage>();
            builder.Services.AddTransient<AppShell>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
