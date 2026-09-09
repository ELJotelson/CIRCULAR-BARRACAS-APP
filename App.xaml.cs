using Circulacion_Barracas.Pages;
using Circulacion_Barracas.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Circulacion_Barracas;

public partial class App : Application
{
    private readonly IServiceProvider services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        this.services = services;

        MainPage = new ContentPage();
        _ = BootstrapAsync();
    }

    private async Task BootstrapAsync()
    {
        var supabase = services.GetRequiredService<SupabaseService>();
        var auth = services.GetRequiredService<AuthService>();

        await supabase.InitializeAsync();

        if (auth.HaySesion && await auth.CargarPerfilAsync())
        {
            var operacionService = services.GetRequiredService<OperacionService>();
            await operacionService.CargarAsync();

            if (operacionService.Actual is not null)
            {
                MainPage = services.GetRequiredService<AppShell>();
            }
            else
            {
                var pagina = services.GetRequiredService<OperacionPage>();
                pagina.AlSeleccionar = () => MainPage = services.GetRequiredService<AppShell>();
                MainPage = new NavigationPage(pagina);
            }
        }
        else
        {
            MainPage = new NavigationPage(services.GetRequiredService<LoginPage>());
        }
    }
}
