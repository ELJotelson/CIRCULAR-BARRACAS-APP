using Circulacion_Barracas.Models;
using Circulacion_Barracas.Services;

namespace Circulacion_Barracas.Pages;

public partial class OperacionPage : ContentPage
{
    private readonly OperacionService operacionService;
    private readonly AuthService auth;
    private readonly SupabaseService supabase;

    public Action? AlSeleccionar { get; set; }

    public OperacionPage(OperacionService operacionService, AuthService auth, SupabaseService supabase)
    {
        InitializeComponent();
        this.operacionService = operacionService;
        this.auth = auth;
        this.supabase = supabase;

        NuevaOperacionCard.IsVisible = auth.EsAdmin;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarAsync();
    }

    private async Task CargarAsync()
    {
        await operacionService.CargarAsync();
        ListaOperaciones.ItemsSource = operacionService.Disponibles;
    }

    private void OnOperacionTapped(object sender, EventArgs e)
    {
        if (sender is not Border border || border.BindingContext is not Operacion operacion)
            return;

        operacionService.Seleccionar(operacion);
        AlSeleccionar?.Invoke();
    }

    private async void OnCrearOperacionClicked(object sender, EventArgs e)
    {
        var nombre = (NombreNuevaEntry.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            await DisplayAlert("Atención", "Escribí un nombre para la operación.", "OK");
            return;
        }

        var ok = await supabase.CrearOperacionAsync(nombre);

        if (!ok)
        {
            await DisplayAlert("Error", "No se pudo crear la operación.", "OK");
            return;
        }

        NombreNuevaEntry.Text = string.Empty;
        await CargarAsync();
    }
}
