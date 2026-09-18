using Circulacion_Barracas.Models;
using Circulacion_Barracas.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Media;

namespace Circulacion_Barracas.Pages;

public partial class NuevoDesvioPage : ContentPage
{
    private readonly DatabaseService db;
    private readonly SupabaseService supabase;
    private readonly AuthService auth;
    private readonly OperacionService operacionService;
    private readonly UpdateService updateService;
    private readonly IServiceProvider services;

    private string fotoPath = string.Empty;

    private List<EmpleadoItem> todosLosEmpleados = new();
    private readonly List<EmpleadoItem> empleadosSeleccionados = new();

    private const string OtroTexto = "OTRO";

    private record EmpleadoItem(string Display, string? Id)
    {
        public override string ToString() => Display;
    }

    public NuevoDesvioPage(DatabaseService db, SupabaseService supabase, AuthService auth,
        OperacionService operacionService, UpdateService updateService, IServiceProvider services)
    {
        InitializeComponent();
        this.db = db;
        this.supabase = supabase;
        this.auth = auth;
        this.operacionService = operacionService;
        this.updateService = updateService;
        this.services = services;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (operacionService.Actual is null)
            return;

        await CargarCatalogosAsync();
        await CargarNominaAsync();
        await CheckForUpdatesAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var latestInfo = await updateService.GetLatestVersionAsync();

            if (latestInfo is null || string.IsNullOrWhiteSpace(latestInfo.Version))
                return;

            string currentVersion = AppInfo.Current.VersionString;

            if (!updateService.IsUpdateAvailable(currentVersion, latestInfo.Version))
                return;

            bool actualizar = await DisplayAlert(
                "Actualización disponible",
                $"Tenés la versión {currentVersion} y hay una nueva versión {latestInfo.Version}.",
                "Actualizar",
                "Después");

            if (actualizar && !string.IsNullOrWhiteSpace(latestInfo.Url))
                await Launcher.Default.OpenAsync(latestInfo.Url);
        }
        catch
        {
            // Si falla internet o la consulta, no hace nada.
        }
    }

    private async Task CargarCatalogosAsync()
    {
        var operacionId = operacionService.Actual!.Id;

        try
        {
            var lugares = await supabase.ObtenerLugaresActivosAsync(operacionId);
            LugarPicker.ItemsSource = lugares.Select(x => x.Nombre).Append(OtroTexto).ToList();

            var tipos = await supabase.ObtenerTiposDesvioActivosAsync(operacionId);
            DesvioPicker.ItemsSource = tipos.Select(x => x.Nombre).Append(OtroTexto).ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar los catálogos: {ex.Message}", "OK");
        }
    }

    private void OnLugarChanged(object sender, EventArgs e) =>
        AplicarSeleccionConOtro(LugarPicker, LugarManualEntry);

    private void OnTipoDesvioChanged(object sender, EventArgs e) =>
        AplicarSeleccionConOtro(DesvioPicker, TipoDesvioManualEntry);

    private static void AplicarSeleccionConOtro(Picker picker, Entry manualEntry)
    {
        bool esOtro = picker.SelectedItem?.ToString() == OtroTexto;
        manualEntry.IsVisible = esOtro;

        if (!esOtro)
            manualEntry.Text = string.Empty;
    }

    private async Task CargarNominaAsync()
    {
        var operacionId = operacionService.Actual!.Id;

        try
        {
            var empleadosCloud = await supabase.ObtenerEmpleadosActivosAsync(operacionId);

            todosLosEmpleados = empleadosCloud
                .Select(e => new EmpleadoItem(e.DisplayName, e.Id.ToString()))
                .ToList();

            db.GuardarNominaLocal(empleadosCloud
                .Select(e => new EmpleadoCache { Legajo = e.Legajo, Nombre = e.Nombre })
                .ToList());
        }
        catch
        {
            todosLosEmpleados = db.ObtenerNominaLocal()
                .Select(e => new EmpleadoItem($"{e.Legajo} - {e.Nombre}", null))
                .ToList();
        }

        ReiniciarSeleccionEmpleado();
    }

    private void ReiniciarSeleccionEmpleado()
    {
        empleadosSeleccionados.Clear();
        ActualizarListaSeleccionados();

        EmpleadoManualGrid.IsVisible = false;
        EmpleadoManualEntry.Text = string.Empty;
        EmpleadoSearchBar.Text = string.Empty;

        MostrarResultadosEmpleado(todosLosEmpleados);
    }

    private void ActualizarListaSeleccionados()
    {
        SeleccionadosList.ItemsSource = null;
        SeleccionadosList.ItemsSource = empleadosSeleccionados;
        SeleccionadosList.IsVisible = empleadosSeleccionados.Count > 0;
    }

    private void MostrarResultadosEmpleado(IEnumerable<EmpleadoItem> resultados)
    {
        var idsSeleccionados = empleadosSeleccionados
            .Where(x => x.Id is not null)
            .Select(x => x.Id)
            .ToHashSet();

        var lista = resultados
            .Where(x => x.Id is null || !idsSeleccionados.Contains(x.Id))
            .ToList();

        lista.Add(new EmpleadoItem(OtroTexto, null));
        ResultadosEmpleadoList.ItemsSource = lista;
    }

    private void OnEmpleadoSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string texto = e.NewTextValue?.Trim().ToLower() ?? string.Empty;

        var filtrados = string.IsNullOrEmpty(texto)
            ? todosLosEmpleados
            : todosLosEmpleados.Where(x => x.Display.ToLower().Contains(texto));

        MostrarResultadosEmpleado(filtrados);
    }

    private void OnEmpleadoResultadoTapped(object sender, EventArgs e)
    {
        if (sender is not Border border || border.BindingContext is not EmpleadoItem item)
            return;

        if (item.Display == OtroTexto)
        {
            EmpleadoManualGrid.IsVisible = true;
            EmpleadoManualEntry.Text = string.Empty;
            EmpleadoManualEntry.Focus();
            return;
        }

        empleadosSeleccionados.Add(item);
        ActualizarListaSeleccionados();

        EmpleadoSearchBar.Text = string.Empty;
        MostrarResultadosEmpleado(todosLosEmpleados);
    }

    private void OnAgregarManualClicked(object sender, EventArgs e)
    {
        var nombre = (EmpleadoManualEntry.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(nombre))
            return;

        empleadosSeleccionados.Add(new EmpleadoItem(nombre, null));
        ActualizarListaSeleccionados();

        EmpleadoManualEntry.Text = string.Empty;
        EmpleadoManualGrid.IsVisible = false;
    }

    private void OnQuitarEmpleadoClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not EmpleadoItem item)
            return;

        empleadosSeleccionados.Remove(item);
        ActualizarListaSeleccionados();

        string texto = EmpleadoSearchBar.Text?.Trim().ToLower() ?? string.Empty;
        var filtrados = string.IsNullOrEmpty(texto)
            ? todosLosEmpleados
            : todosLosEmpleados.Where(x => x.Display.ToLower().Contains(texto));

        MostrarResultadosEmpleado(filtrados);
    }

    private async void OnTomarFotoClicked(object sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await DisplayAlert("Error", "La cámara no está disponible.", "OK");
                return;
            }

            var foto = await MediaPicker.Default.CapturePhotoAsync();
            await GuardarFotoSeleccionadaAsync(foto);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo tomar la foto: {ex.Message}", "OK");
        }
    }

    private async void OnElegirDeGaleriaClicked(object sender, EventArgs e)
    {
        try
        {
            var foto = await MediaPicker.Default.PickPhotoAsync();
            await GuardarFotoSeleccionadaAsync(foto);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo elegir la foto: {ex.Message}", "OK");
        }
    }

    private async Task GuardarFotoSeleccionadaAsync(FileResult? foto)
    {
        if (foto is null)
            return;

        var nuevoNombre = $"{Guid.NewGuid()}.jpg";
        var destino = Path.Combine(FileSystem.AppDataDirectory, nuevoNombre);

        using var streamOrigen = await foto.OpenReadAsync();
        using var streamDestino = File.OpenWrite(destino);
        await streamOrigen.CopyToAsync(streamDestino);

        fotoPath = destino;

        FotoPreview.Source = ImageSource.FromFile(destino);
        FotoPreview.IsVisible = true;
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        if (operacionService.Actual is null)
        {
            await DisplayAlert("Atención", "Elegí una operación antes de cargar un desvío.", "OK");
            return;
        }

        if (empleadosSeleccionados.Count == 0)
        {
            await DisplayAlert("Atención", "Tenés que elegir o agregar al menos un empleado.", "OK");
            return;
        }

        var operacionId = operacionService.Actual.Id.ToString();
        var inspector = auth.NombreMostrado;
        var lugar = ValorFinal(LugarPicker, LugarManualEntry);
        var tipoDesvio = ValorFinal(DesvioPicker, TipoDesvioManualEntry);
        var observaciones = ObservacionesEditor.Text ?? string.Empty;
        var fecha = DateTime.Now;
        var userId = auth.UsuarioActualId?.ToString();

        try
        {
            GuardarButton.IsEnabled = false;
            GuardandoIndicator.IsVisible = true;
            GuardandoIndicator.IsRunning = true;

            int guardadosEnNube = 0;

            foreach (var empleado in empleadosSeleccionados)
            {
                var desvio = new Desvio
                {
                    Inspector = inspector,
                    Empleado = empleado.Display,
                    EmpleadoId = empleado.Id,
                    Lugar = lugar,
                    TipoDesvio = tipoDesvio,
                    Observaciones = observaciones,
                    FotoPath = fotoPath,
                    Fecha = fecha,
                    UserId = userId,
                    OperacionId = operacionId
                };

                db.GuardarDesvio(desvio);

                if (await supabase.GuardarDesvioAsync(desvio))
                    guardadosEnNube++;
            }

            if (guardadosEnNube == empleadosSeleccionados.Count)
                await DisplayAlert("Listo", $"Se guardaron {guardadosEnNube} desvío(s), local y en la nube.", "OK");
            else
                await DisplayAlert("Atención", $"Se guardaron los {empleadosSeleccionados.Count} desvíos local, pero solo {guardadosEnNube} llegaron a la nube por ahora.", "OK");

            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
        }
        finally
        {
            GuardarButton.IsEnabled = true;
            GuardandoIndicator.IsVisible = false;
            GuardandoIndicator.IsRunning = false;
        }
    }

    private static string ValorFinal(Picker picker, Entry manualEntry) =>
        picker.SelectedItem?.ToString() == OtroTexto
            ? (manualEntry.Text ?? string.Empty).Trim()
            : picker.SelectedItem?.ToString() ?? string.Empty;

    private void LimpiarFormulario()
    {
        LugarPicker.SelectedIndex = -1;
        LugarManualEntry.IsVisible = false;
        LugarManualEntry.Text = string.Empty;

        DesvioPicker.SelectedIndex = -1;
        TipoDesvioManualEntry.IsVisible = false;
        TipoDesvioManualEntry.Text = string.Empty;

        ObservacionesEditor.Text = string.Empty;

        fotoPath = string.Empty;
        FotoPreview.Source = null;
        FotoPreview.IsVisible = false;

        ReiniciarSeleccionEmpleado();
    }

    private async void OnVerListaClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(services.GetRequiredService<ListaDesviosPage>());
    }
}
