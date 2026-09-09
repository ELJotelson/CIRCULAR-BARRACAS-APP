using Circulacion_Barracas.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Circulacion_Barracas.Pages;

public partial class SignUpPage : ContentPage
{
    private readonly AuthService auth;
    private readonly OperacionService operacionService;
    private readonly IServiceProvider services;

    public SignUpPage(AuthService auth, OperacionService operacionService, IServiceProvider services)
    {
        InitializeComponent();
        this.auth = auth;
        this.operacionService = operacionService;
        this.services = services;
    }

    private async void OnCrearCuentaClicked(object sender, EventArgs e)
    {
        var nombre = (NombreEntry.Text ?? string.Empty).Trim();
        var email = (EmailEntry.Text ?? string.Empty).Trim();
        var password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            MostrarError("Completá todos los campos.");
            return;
        }

        if (password.Length < 6)
        {
            MostrarError("La contraseña tiene que tener al menos 6 caracteres.");
            return;
        }

        ErrorLabel.IsVisible = false;
        CrearCuentaButton.IsEnabled = false;
        CargandoIndicator.IsVisible = true;
        CargandoIndicator.IsRunning = true;

        var (error, requiereConfirmacion) = await auth.RegistrarseAsync(email, password, nombre);

        CargandoIndicator.IsVisible = false;
        CargandoIndicator.IsRunning = false;
        CrearCuentaButton.IsEnabled = true;

        if (error is not null)
        {
            MostrarError(error);
            return;
        }

        if (requiereConfirmacion)
        {
            await DisplayAlert("Casi listo", "Te enviamos un email para confirmar la cuenta. Confirmalo y después iniciá sesión.", "OK");
            await Navigation.PopAsync();
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

    private async void OnVolverALoginClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void MostrarError(string mensaje)
    {
        ErrorLabel.Text = mensaje;
        ErrorLabel.IsVisible = true;
    }
}
