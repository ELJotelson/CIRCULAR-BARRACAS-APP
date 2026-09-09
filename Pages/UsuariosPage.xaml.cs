using System.Collections.ObjectModel;
using Circulacion_Barracas.Models;
using Circulacion_Barracas.Services;

namespace Circulacion_Barracas.Pages;

public partial class UsuariosPage : ContentPage
{
    private readonly SupabaseService supabase;
    private readonly AuthService auth;
    private readonly ObservableCollection<Profile> lista = new();

    public UsuariosPage(SupabaseService supabase, AuthService auth)
    {
        InitializeComponent();
        this.supabase = supabase;
        this.auth = auth;
        ListaUsuarios.ItemsSource = lista;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarAsync();
    }

    private async Task CargarAsync()
    {
        try
        {
            var perfiles = await supabase.ObtenerPerfilesAsync();
            lista.Clear();
            foreach (var perfil in perfiles)
                lista.Add(perfil);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar los usuarios: {ex.Message}", "OK");
        }
    }

    private async void OnCambiarRolClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not Profile perfil)
            return;

        if (perfil.Id == auth.UsuarioActualId)
        {
            await DisplayAlert("Atención", "No podés cambiar tu propio rol desde acá.", "OK");
            return;
        }

        var nuevoRol = perfil.EsAdmin ? "inspector" : "admin";
        var nombre = string.IsNullOrWhiteSpace(perfil.NombreCompleto) ? perfil.Email ?? "este usuario" : perfil.NombreCompleto;

        bool confirmar = await DisplayAlert(
            "Cambiar rol",
            $"¿Cambiar a {nombre} a \"{nuevoRol}\"?",
            "Confirmar",
            "Cancelar");

        if (!confirmar)
            return;

        var (ok, error) = await supabase.CambiarRolAsync(perfil, nuevoRol);

        if (!ok)
        {
            await DisplayAlert("Error", $"No se pudo cambiar el rol.\n\n{error}", "OK");
            return;
        }

        await CargarAsync();
    }
}
