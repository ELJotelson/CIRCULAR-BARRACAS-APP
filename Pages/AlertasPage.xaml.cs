using System.Collections.ObjectModel;
using Circulacion_Barracas.Models;
using Circulacion_Barracas.Services;
using Supabase.Realtime;

namespace Circulacion_Barracas.Pages;

public partial class AlertasPage : ContentPage
{
    private readonly SupabaseService supabase;
    private readonly OperacionService operacionService;
    private readonly ObservableCollection<Desvio> lista = new();
    private RealtimeChannel? canal;

    public AlertasPage(SupabaseService supabase, OperacionService operacionService)
    {
        InitializeComponent();
        this.supabase = supabase;
        this.operacionService = operacionService;
        ListaAlertas.ItemsSource = lista;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (operacionService.Actual is null)
            return;

        var operacionId = operacionService.Actual.Id;

        try
        {
            lista.Clear();
            var ultimos = await supabase.ObtenerUltimosDesviosAsync(operacionId, 30);
            foreach (var desvio in ultimos)
                lista.Add(desvio);

            canal ??= await supabase.SuscribirseANuevosDesviosAsync(nuevo =>
            {
                if (nuevo.OperacionId != operacionId.ToString())
                    return;

                MainThread.BeginInvokeOnMainThread(() => lista.Insert(0, nuevo));
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las alertas: {ex.Message}", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        canal?.Unsubscribe();
        canal = null;
    }
}
