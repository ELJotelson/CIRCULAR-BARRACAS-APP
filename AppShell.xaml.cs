using Circulacion_Barracas.Pages;
using Circulacion_Barracas.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Circulacion_Barracas;

public partial class AppShell : Shell
{
    private readonly AuthService auth;
    private readonly OperacionService operacionService;
    private readonly IServiceProvider services;

    public AppShell(AuthService auth, OperacionService operacionService, IServiceProvider services)
    {
        InitializeComponent();
        this.auth = auth;
        this.operacionService = operacionService;
        this.services = services;

        SetFlyoutItemIsVisible(CatalogosShellContent, auth.EsAdmin);
        SetFlyoutItemIsVisible(MotivosShellContent, auth.EsAdmin);
        SetFlyoutItemIsVisible(UsuariosShellContent, auth.EsAdmin);

        UsuarioActualLabel.Text = auth.EmailActual ?? "—";
        RolActualLabel.Text = auth.EsAdmin ? "ADMINISTRADOR" : "INSPECTOR";

        ActualizarLabelOperacion();
        operacionService.CambioOperacion += ActualizarLabelOperacion;
    }

    private void ActualizarLabelOperacion()
    {
        OperacionActualLabel.Text = operacionService.Actual?.Nombre ?? "Elegir operación";
    }

    private async void OnCambiarOperacionTapped(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;

        var pagina = services.GetRequiredService<OperacionPage>();
        pagina.AlSeleccionar = async () => await Navigation.PopModalAsync();

        await Navigation.PushModalAsync(new NavigationPage(pagina));
    }

    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        await auth.CerrarSesionAsync();
        Application.Current!.MainPage = new NavigationPage(services.GetRequiredService<LoginPage>());
    }
}
