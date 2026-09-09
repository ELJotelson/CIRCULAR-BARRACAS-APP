using System.Collections.ObjectModel;
using Circulacion_Barracas.Models;
using Circulacion_Barracas.Services;

namespace Circulacion_Barracas.Pages;

public partial class CatalogosPage : ContentPage
{
    private readonly SupabaseService supabase;
    private readonly OperacionService operacionService;
    private readonly ObservableCollection<ICatalogoItem> items = new();

    private enum TipoCatalogo { Lugares, TiposDesvio }
    private TipoCatalogo catalogoActual = TipoCatalogo.Lugares;

    public CatalogosPage(SupabaseService supabase, OperacionService operacionService)
    {
        InitializeComponent();
        this.supabase = supabase;
        this.operacionService = operacionService;

        ListaItems.ItemsSource = items;
        CatalogoPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarAsync();
    }

    private async void OnCatalogoChanged(object sender, EventArgs e)
    {
        catalogoActual = (TipoCatalogo)CatalogoPicker.SelectedIndex;
        await CargarAsync();
    }

    private async Task CargarAsync()
    {
        if (operacionService.Actual is null)
            return;

        var operacionId = operacionService.Actual.Id;
        var resultado = await ObtenerCatalogoActualAsync(operacionId);

        items.Clear();
        foreach (var item in resultado)
            items.Add(item);
    }

    private async Task<List<ICatalogoItem>> ObtenerCatalogoActualAsync(Guid operacionId)
    {
        if (catalogoActual == TipoCatalogo.Lugares)
            return (await supabase.ObtenerLugaresActivosAsync(operacionId)).Cast<ICatalogoItem>().ToList();

        return (await supabase.ObtenerTiposDesvioActivosAsync(operacionId)).Cast<ICatalogoItem>().ToList();
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

        var operacionId = operacionService.Actual.Id;

        bool ok = catalogoActual == TipoCatalogo.Lugares
            ? await supabase.AgregarLugarAsync(operacionId, nombre)
            : await supabase.AgregarTipoDesvioAsync(operacionId, nombre);

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
        if (sender is not SwipeItem swipeItem || swipeItem.BindingContext is not ICatalogoItem item)
            return;

        bool confirmar = await DisplayAlert("Desactivar", $"¿Desactivar \"{item.Nombre}\"?", "Desactivar", "Cancelar");

        if (!confirmar)
            return;

        bool ok = catalogoActual == TipoCatalogo.Lugares
            ? await supabase.DesactivarLugarAsync((Lugar)item)
            : await supabase.DesactivarTipoDesvioAsync((TipoDesvio)item);

        if (!ok)
        {
            await DisplayAlert("Error", "No se pudo desactivar.", "OK");
            return;
        }

        items.Remove(item);
    }
}
