using Microsoft.Maui.Media;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.ApplicationModel;

namespace Circulacion_Barracas;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService db;
    private readonly SupabaseService supabase;
    private readonly UpdateService updateService;
    private string fotoPath = string.Empty;
    private List<string> todosLosEmpleados = new();
    private List<string> empleadosFiltrados = new();

    public MainPage()
    {
        InitializeComponent();

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "desvios.db3");
        db = new DatabaseService(dbPath);
        supabase = new SupabaseService();
        updateService = new UpdateService();

        CargarOpciones();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CheckForUpdatesAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var latestInfo = await updateService.GetLatestVersionAsync();

            if (latestInfo == null || string.IsNullOrWhiteSpace(latestInfo.Version))
                return;

            string currentVersion = AppInfo.Current.VersionString;

            bool updateAvailable = updateService.IsUpdateAvailable(currentVersion, latestInfo.Version);

            if (!updateAvailable)
                return;

            bool actualizar = await DisplayAlert(
                "Actualización disponible",
                $"Tenés la versión {currentVersion} y hay una nueva versión {latestInfo.Version}.",
                "Actualizar",
                "Después");

            if (actualizar && !string.IsNullOrWhiteSpace(latestInfo.Url))
            {
                await Launcher.Default.OpenAsync(latestInfo.Url);
            }
        }
        catch
        {
            // Si falla internet o la consulta, no hace nada
        }
    }

    private void CargarOpciones()
    {
        InspectorPicker.ItemsSource = new List<string>
        {
            "SEGURIDAD PATRIMONIAL",
            "LEANDRO DEL VECCHIO",
            "NICOLAS BASUALDO",
            "MAURO FONTAN",
            "MAURICIO VAZQUEZ",
            "OTRO"
        };

        LugarPicker.ItemsSource = new List<string>
        {
            "DEPOSITO",
            "PLAYA CARGA/DESCARGA",
            "OFICINAS ADM",
            "COMEDOR",
            "OTRO"
        };

        todosLosEmpleados = new List<string>
        {
            "116 - VAZQUEZ MAURICIO FEDERICO",
            "204 - LUPERINI ADRIAN ANDRES",
            "206 - VARGAS RODRIGUEZ ELOY",
            "209 - CASTRO CARLOS FERNANDO",
            "215 - MARTINEZ CARDOZO ROBERTO GUSTAVO",
            "217 - HERRERA FELIX LEONICIO",
            "226 - LOPEZ PABLO JOSE",
            "229 - VALLEJO SANDOVAL JOSE MARTIN",
            "231 - PALACIOS IVAN",
            "233 - SORIA CARLOS GUSTAVO",
            "234 - GONZALEZ JUAN CRUZ",
            "235 - RODRIGUEZ JUAN JOSE",
            "236 - MAZA ENRIQUE DANIEL",
            "240 - MORALES JUAN CARLOS",
            "241 - MARTINEZ GASTON EZEQUIEL",
            "242 - FERNANDEZ JORGE ALBERTO",
            "243 - PERALTA JOSE LUIS",
            "245 - GARCIA JAVIER ALEJANDRO",
            "246 - SANTILLAN ROBERTO",
            "247 - RUIZ DIAZ MIGUEL ANGEL",
            "248 - GOMEZ CLAUDIO DANIEL",
            "249 - FLORES RUBEN DARIO",
            "250 - GONZALEZ RICARDO DANIEL",
            "251 - SOSA HECTOR ALBERTO",
            "252 - BARRIONUEVO ARIEL ALEJANDRO",
            "253 - NUÑEZ SERGIO DANIEL",
            "254 - PEREZ CRISTIAN DAVID",
            "255 - ACOSTA LUIS ALBERTO",
            "256 - BENITEZ CESAR ALBERTO",
            "257 - CABRERA MARIO ALBERTO",
            "258 - CORONEL JORGE OMAR",
            "259 - DIAZ HUGO ARIEL",
            "260 - ESCOBAR DARIO FABIAN",
            "261 - FARIAS JORGE DANIEL",
            "262 - GIMENEZ LUIS ALBERTO",
            "263 - HERRERA RUBEN DARIO",
            "264 - IBARRA CARLOS DANIEL",
            "265 - JUAREZ FABIAN ALBERTO",
            "266 - KRAMER MARCELO GUSTAVO",
            "267 - LEDESMA PABLO ANDRES",
            "268 - MEDINA MIGUEL ANGEL",
            "269 - NAVARRO DIEGO MARTIN",
            "270 - OJEDA SERGIO DANIEL",
            "271 - PONCE CLAUDIO FABIAN",
            "272 - QUIROGA WALTER DAVID",
            "273 - RAMIREZ EDUARDO DANIEL",
            "274 - SALAZAR HUGO ALBERTO",
            "275 - TORRES GABRIEL ALEJANDRO",
            "276 - URQUIZA CRISTIAN DAVID",
            "277 - VERA MIGUEL ANGEL",
            "278 - WALTER JOSE LUIS",
            "279 - XXXXXX EMPLEADO",
            "280 - YYYYYY EMPLEADO"
        };

        empleadosFiltrados = new List<string>(todosLosEmpleados);
        EmpleadoPicker.ItemsSource = empleadosFiltrados;
    }

    private void OnEmpleadoSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string texto = e.NewTextValue?.Trim().ToLower() ?? string.Empty;

        empleadosFiltrados = todosLosEmpleados
            .Where(x => x.ToLower().Contains(texto))
            .ToList();

        EmpleadoPicker.ItemsSource = null;
        EmpleadoPicker.ItemsSource = empleadosFiltrados;
    }

    private async void OnTomarFotoClicked(object sender, EventArgs e)
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var foto = await MediaPicker.Default.CapturePhotoAsync();

                if (foto != null)
                {
                    var nuevoNombre = $"{Guid.NewGuid()}.jpg";
                    var destino = Path.Combine(FileSystem.AppDataDirectory, nuevoNombre);

                    using var streamOrigen = await foto.OpenReadAsync();
                    using var streamDestino = File.OpenWrite(destino);
                    await streamOrigen.CopyToAsync(streamDestino);

                    fotoPath = destino;

                    FotoPreview.Source = ImageSource.FromFile(destino);
                    FotoPreview.IsVisible = true;
                }
            }
            else
            {
                await DisplayAlert("Error", "La cámara no está disponible.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo tomar la foto: {ex.Message}", "OK");
        }
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        try
        {
            var desvio = new Desvio
            {
                Inspector = InspectorPicker.SelectedItem?.ToString() ?? string.Empty,
                Empleado = EmpleadoPicker.SelectedItem?.ToString() ?? string.Empty,
                Lugar = LugarPicker.SelectedItem?.ToString() ?? string.Empty,
                TipoDesvio = DesvioPicker.SelectedItem?.ToString() ?? string.Empty,
                Observaciones = ObservacionesEditor.Text ?? string.Empty,
                FotoPath = fotoPath,
                Fecha = DateTime.Now
            };

            db.GuardarDesvio(desvio);

            bool guardadoNube = await supabase.GuardarDesvioAsync(desvio);

            if (guardadoNube)
                await DisplayAlert("OK", "Desvío guardado local y en la nube", "OK");
            else
                await DisplayAlert("Atención", "Se guardó local, pero no en la nube", "OK");

            InspectorPicker.SelectedIndex = -1;
            EmpleadoPicker.SelectedIndex = -1;
            LugarPicker.SelectedIndex = -1;
            DesvioPicker.SelectedIndex = -1;
            ObservacionesEditor.Text = string.Empty;
            EmpleadoSearchBar.Text = string.Empty;

            fotoPath = string.Empty;
            FotoPreview.Source = null;
            FotoPreview.IsVisible = false;

            empleadosFiltrados = new List<string>(todosLosEmpleados);
            EmpleadoPicker.ItemsSource = null;
            EmpleadoPicker.ItemsSource = empleadosFiltrados;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
        }
    }

    private async void OnVerListaClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListaDesviosPage());
    }
}