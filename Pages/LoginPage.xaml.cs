using Circulacion_Barracas.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Circulacion_Barracas.Pages;

public partial class LoginPage : ContentPage
{
    private readonly AuthService auth;
    private readonly OperacionService operacionService;
    private readonly IServiceProvider services;

    public LoginPage(AuthService auth, OperacionService operacionService, IServiceProvider services)
    {
        InitializeComponent();
        this.auth = auth;
        this.operacionService = operacionService;
        this.services = services;
    }

    private async void OnIngresarClicked(object sender, EventArgs e)
    {
        var email = (EmailEntry.Text ?? string.Empty).Trim();
        var password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            MostrarError("Completá email y contraseña.");
            return;
        }

        ErrorLabel.IsVisible = false;
        IngresarButton.IsEnabled = false;
        CargandoIndicator.IsVisible = true;
        CargandoIndicator.IsRunning = true;

        var error = await auth.IniciarSesionAsync(email, password);

        CargandoIndicator.IsVisible = false;
        CargandoIndicator.IsRunning = false;
        IngresarButton.IsEnabled = true;

        if (error is not null)
        {
            MostrarError(error);
            return;
        }

        await operacionService.CargarAsync();

        if (operacionService.Actual is not null)
        {
            Application.Current!.MainPage = services.GetRequiredService<AppShell>();
        }
        else
        {
            var pagina = services.GetRequiredService<OperacionPage>();
            pagina.AlSeleccionar = () => Application.Current!.MainPage = services.GetRequiredService<AppShell>();
            Application.Current!.MainPage = new NavigationPage(pagina);
        }
    }

    private async void OnOlvideClicked(object sender, EventArgs e)
    {
        var email = (EmailEntry.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            MostrarError("Escribí tu email arriba y volvé a tocar el link.");
            return;
        }

        var error = await auth.RecuperarContrasenaAsync(email);

        await DisplayAlert(
            "Recuperar contraseña",
            error is null ? "Te enviamos un email para restablecer tu contraseña." : error,
            "OK");
    }

    private async void OnCrearCuentaClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(services.GetRequiredService<SignUpPage>());
    }

    private void MostrarError(string mensaje)
    {
        ErrorLabel.Text = mensaje;
        ErrorLabel.IsVisible = true;
    }
}
