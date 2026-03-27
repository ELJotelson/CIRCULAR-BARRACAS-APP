using Microsoft.Maui.Media;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;

namespace Circulacion_Barracas;

public partial class MainPage : ContentPage
{
    private readonly HttpClient httpClient = new();
    private string fotoPath = string.Empty;

    // CAMBIÁ ESTA IP Y ESTE PUERTO POR LOS DE TU API
    private const string ApiUrl = "http://192.168.3.162:7037/api/Desvios";

    private List<string> todosLosEmpleados = new();
    private List<string> empleadosFiltrados = new();

    public MainPage()
    {
        InitializeComponent();
        CargarOpciones();
    }

    private void CargarOpciones()
    {
        InspectorPicker.ItemsSource = new List<string>
        {
            "Inspector 1",
            "Inspector 2",
            "Inspector 3"
        };

        LugarPicker.ItemsSource = new List<string>
        {
            "Barracas",
            "Constitución",
            "Avellaneda"
        };

        todosLosEmpleados = new List<string>
        {
            "116 - VAZQUEZ MAURICIO FEDERICO",
            "204 - LUPERINI ADRIAN ANDRES",
            "206 - VARGAS RODRIGUEZ ELOY",
            "209 - CASTRO CARLOS FERNANDO",
            "215 - MARTINEZ CARDOZO ROBERTO GUSTAVO",
            "217 - HERRERA FELIX LEONICIO",
            "226 - SAMANIEGO JUSTINO CIPRIANO",
            "233 - PANIAGUA CACERES ERNESTO ISRAEL",
            "234 - BARRIENTOS RENE ARGENTINO",
            "238 - MENDEZ MARCELO GUSTAVO",
            "240 - CAMPERO HERNAN ALEJANDRO",
            "241 - BOHL FERNANDO ANDRES",
            "242 - LOPEZ CARLOS CLEMENTE",
            "245 - MELIAN DIEGO MAXIMILIANO",
            "248 - SPARACINO PABLO DANIEL",
            "250 - MARTINEZ LORENA ELISA",
            "253 - PAVON LUCILA JAZMIN",
            "255 - RAMERO BELEN GABRIELA",
            "257 - SURBANDO ARIEL IGNACIO",
            "258 - MASTROIANNI MARCO ANTONIO",
            "259 - SENES CRISTIAN ADRIEL",
            "262 - FERNANDEZ DANIEL ANTONIO",
            "263 - LATTANZI DIEGO OMAR",
            "265 - PALAVECINO EDWIN EZEQUIEL",
            "304 - BORDON SERGIO CESAR",
            "307 - GALVAN MARIO GABRIEL",
            "309 - GUTIERREZ ALFREDO HORACIO",
            "312 - LUQUE RAMON ROGELIO",
            "313 - MARCELLO FERNANDO HORACIO",
            "317 - SANDRIGO ALBERTO",
            "318 - SANDRIGO DIEGO SEBASTIAN",
            "321 - FERREYRA JORGE ALEJANDRO",
            "322 - CREGO CREGO GUSTAVO",
            "327 - DELGADO JORGE OMAR",
            "328 - FERNANDEZ DANIEL ABEL",
            "329 - GUTIERREZ RUBEN",
            "330 - MARTINEZ EZEQUIEL MAXIMILIANO",
            "332 - SAAVEDRA HUGO OSVALDO",
            "333 - PIÑEYRO SILVESTRE",
            "335 - MILLA JORGE LEANDRO",
            "337 - ACOSTA CARLOS EDUARDO",
            "339 - ZARACHO ROBERTO",
            "342 - LEDESMA CRISTIAN DAMIAN",
            "345 - ACUÑA ALBERTO",
            "346 - AYBAR SEBASTIAN",
            "348 - CERDAN GASTON",
            "349 - COLOMBO MAXIMILIANO EZEQUIEL",
            "350 - DOMINGUEZ CESAR",
            "354 - CEJAS SEBASTIAN",
            "355 - CISNEROS MIGUEL ANGEL",
            "356 - GAZANO RAFAEL",
            "358 - PARED RAMON FERNANDO",
            "359 - PELOZO MARIANO RODRIGO",
            "360 - PERALTA MARCOS DANIEL",
            "361 - SANCHEZ RENE ORLANDO",
            "366 - GOMEZ ALBERTO",
            "370 - PIRILLO JOSE",
            "371 - VALDEZ DIEGO",
            "373 - VARGAS EDUARDO",
            "375 - MENCHACA JORGE DANIEL",
            "378 - CASTAÑO DANIEL AGUSTIN",
            "381 - FERNANDEZ OSCAR ALBERTO",
            "382 - GILES FERNANDO",
            "383 - MARTINEZ NICOLAS",
            "384 - MEDINA MEDINA DIEGO",
            "466 - CEJAS EZEQUIEL",
            "467 - COLOMBO MATIAS SEBASTIAN",
            "468 - FERNANDEZ JAVIER REINALDO",
            "469 - GALLARDO JUAN GABRIEL",
            "472 - OLIBA JUAN MANUEL",
            "473 - PIRILLO CLAUDIO LEANDRO",
            "474 - POGONZA JORGE DAMIAN",
            "475 - ROSALES FABIAN",
            "476 - SABATTINO ESTEBAN",
            "477 - RAMIREZ RUBEN DARIO",
            "478 - QUINTANA JUAN ALBERTO",
            "480 - LUNA OSCAR",
            "481 - PEÑALOZA DARIO",
            "482 - MORENO POBLETE RAFAEL",
            "483 - IBARRA ALEJANDRO",
            "484 - MOLINA EXEQUIEL ANDRES",
            "485 - GILES DAMIAN MATIAS",
            "486 - CERDAN MATIAS NICOLAS",
            "487 - GOMEZ JORGE RICARDO",
            "488 - BAEZ ALEXIS EZEQUIEL",
            "489 - FERNANDEZ CARLOS",
            "490 - ALBOR ALBOR HERNAN",
            "492 - CORONEL DAVID RODRIGO",
            "493 - MANSILLA RAMON",
            "494 - GALVAN LUIS ALBERTO",
            "495 - PEREYRA GUILLERMO WALTER",
            "496 - PEÑALOZA SERGIO LEONEL",
            "497 - BRUMAT ISMAEL",
            "498 - SUAREZ SUAREZ MATIAS",
            "499 - GUARASCI MARCELO FABIAN",
            "500 - BENITEZ HUGO MARCIAL",
            "501 - FARIAS CLAUDIO ALEJANDRO",
            "502 - HERNANDEZ LUIS ALBERTO",
            "503 - PANOSSIAN ESTEBAN HERNAN",
            "504 - SALAZAR SEBASTIAN DARIO",
            "505 - GAZCON ARIEL EZEQUIEL",
            "506 - RODRIGUEZ DIEGO JAVIER",
            "507 - VARGAS GONZALO NICOLAS"
        };

        empleadosFiltrados = new List<string>(todosLosEmpleados);
        EmpleadoPicker.ItemsSource = empleadosFiltrados;
    }

    private void OnEmpleadoSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string texto = e.NewTextValue?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(texto))
            empleadosFiltrados = new List<string>(todosLosEmpleados);
        else
            empleadosFiltrados = todosLosEmpleados
                .Where(x => x.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .ToList();

        EmpleadoPicker.ItemsSource = null;
        EmpleadoPicker.ItemsSource = empleadosFiltrados;

        if (empleadosFiltrados.Count > 0)
            EmpleadoPicker.SelectedIndex = 0;
    }

    private async void OnTomarFotoClicked(object sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await DisplayAlert("Error", "La cámara no está disponible en este dispositivo.", "OK");
                return;
            }

            var foto = await MediaPicker.Default.CapturePhotoAsync();

            if (foto == null)
                return;

            string nombreArchivo = $"{DateTime.Now:yyyyMMddHHmmss}.jpg";
            string destino = Path.Combine(FileSystem.AppDataDirectory, nombreArchivo);

            using var sourceStream = await foto.OpenReadAsync();
            using var fileStream = File.OpenWrite(destino);
            await sourceStream.CopyToAsync(fileStream);

            fotoPath = destino;
            FotoPreview.Source = ImageSource.FromFile(destino);
            FotoPreview.IsVisible = true;
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

            var response = await httpClient.PostAsJsonAsync(ApiUrl, desvio);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"No se pudo guardar en la API.\n{error}", "OK");
                return;
            }

            await DisplayAlert("OK", "Desvío guardado en la base central", "OK");

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