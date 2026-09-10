using System.Collections.ObjectModel;
using Circulacion_Barracas.Models;
using Circulacion_Barracas.Services;

namespace Circulacion_Barracas.Pages;

public partial class MotivosDesvioPage : ContentPage
{
    private readonly SupabaseService supabase;
    private readonly OperacionService operacionService;
    private readonly ObservableCollection<TipoDesvio> items = new();

    public MotivosDesvioPage(SupabaseService supabase, OperacionService operacionService)
    {
        InitializeComponent();
        this.supabase = supabase;
        this.operacionService = operacionService;

        ListaItems.ItemsSource = items;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarAsync();
    }

    private async Task CargarAsync()
    {
        if (operacionService.Actual is null)
            return;

        var resultado = await supabase.ObtenerTiposDesvioActivosAsync(operacionService.Actual.Id);

        items.Clear();
        foreach (var item in resultado)
            items.Add(item);
    }

    private async void OnAgregarClicked(object sender, EventArgs e)
    {
        if (operacionService.Actual is null)
        {
            await DisplayAlert("Atención", "Elegí una operación primero.", "OK");
            return;
        }

        var nombre = (NombreNuevoEntry.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            await DisplayAlert("Atención", "Escribí un nombre.", "OK");
            return;
        }

        bool ok = await supabase.AgregarTipoDesvioAsync(operacionService.Actual.Id, nombre);

        if (!ok)
        {
            await DisplayAlert("Error", "No se pudo agregar.", "OK");
            return;
        }

        NombreNuevoEntry.Text = string.Empty;
        await CargarAsync();
    }

    private async void OnDesactivarSwipe(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem || swipeItem.BindingContext is not TipoDesvio tipo)
            return;

        bool confirmar = await DisplayAlert("Desactivar", $"¿Desactivar \"{tipo.Nombre}\"?", "Desactivar", "Cancelar");

        if (!confirmar)
            return;

        bool ok = await supabase.DesactivarTipoDesvioAsync(tipo);

        if (!ok)
        {
            await DisplayAlert("Error", "No se pudo desactivar.", "OK");
            return;
        }

        items.Remove(tipo);
    }
}
