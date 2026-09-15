using System.Collections.ObjectModel;
using ClosedXML.Excel;
using Circulacion_Barracas.Models;
using Circulacion_Barracas.Services;

namespace Circulacion_Barracas.Pages;

public partial class NominaPage : ContentPage
{
    private readonly SupabaseService supabase;
    private readonly OperacionService operacionService;
    private readonly AuthService auth;
    private readonly ObservableCollection<Empleado> lista = new();
    private List<Empleado> todosLosEmpleados = new();

    private static readonly FilePickerFileType NominaFileType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.iOS, new[] { "public.spreadsheet", "public.comma-separated-values-text" } },
        { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "text/csv", "text/comma-separated-values" } },
        { DevicePlatform.WinUI, new[] { ".xlsx", ".csv" } },
        { DevicePlatform.MacCatalyst, new[] { "xlsx", "csv" } },
    });

    public NominaPage(SupabaseService supabase, OperacionService operacionService, AuthService auth)
    {
        InitializeComponent();
        this.supabase = supabase;
        this.operacionService = operacionService;
        this.auth = auth;
        ListaEmpleados.ItemsSource = lista;

        ImportarSection.IsVisible = auth.EsAdmin;
        SubtituloLabel.Text = auth.EsAdmin
            ? "Importá un Excel (.xlsx) o CSV con columnas Legajo y Nombre. Se actualiza por legajo, no duplica."
            : "Buscá a un empleado por nombre o legajo.";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarListaAsync();
    }

    private async Task CargarListaAsync()
    {
        if (operacionService.Actual is null)
            return;

        try
        {
            todosLosEmpleados = await supabase.ObtenerEmpleadosActivosAsync(operacionService.Actual.Id);
            AplicarFiltro(BuscarEmpleadoSearchBar.Text);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar la nómina: {ex.Message}", "OK");
        }
    }

    private void OnBuscarTextChanged(object sender, TextChangedEventArgs e) => AplicarFiltro(e.NewTextValue);

    private void AplicarFiltro(string? texto)
    {
        texto = texto?.Trim().ToLower() ?? string.Empty;

        var filtrados = string.IsNullOrEmpty(texto)
            ? todosLosEmpleados
            : todosLosEmpleados.Where(x => x.Nombre.ToLower().Contains(texto) || x.Legajo.ToLower().Contains(texto));

        lista.Clear();
        foreach (var empleado in filtrados)
            lista.Add(empleado);
    }

    private async void OnImportarClicked(object sender, EventArgs e)
    {
        if (operacionService.Actual is null)
        {
            await DisplayAlert("Atención", "Elegí una operación primero.", "OK");
            return;
        }

        var operacionId = operacionService.Actual.Id;

        try
        {
            var archivo = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Elegí el archivo de nómina (.xlsx o .csv)",
                FileTypes = NominaFileType
            });

            if (archivo is null)
                return;

            ImportarButton.IsEnabled = false;
            ImportandoIndicator.IsVisible = true;
            ImportandoIndicator.IsRunning = true;
            ResultadoLabel.IsVisible = false;

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            using var stream = await archivo.OpenReadAsync();
            var filas = extension == ".csv" ? ParsearCsv(stream) : ParsearExcel(stream);

            if (filas.Count == 0)
            {
                await DisplayAlert("Nómina", "No se encontraron filas válidas (se esperan columnas Legajo y Nombre).", "OK");
                return;
            }

            var existentes = await supabase.ObtenerEmpleadosAsync(operacionId);
            var existentesPorLegajo = existentes.ToDictionary(x => x.Legajo, StringComparer.OrdinalIgnoreCase);

            var empleados = filas
                .Select(f => new Empleado
                {
                    Id = existentesPorLegajo.TryGetValue(f.Legajo, out var existente) ? existente.Id : Guid.NewGuid(),
                    OperacionId = operacionId,
                    Legajo = f.Legajo,
                    Nombre = f.Nombre,
                    Activo = true
                })
                .ToList();

            var cantidad = await supabase.ImportarNominaAsync(empleados);

            ResultadoLabel.Text = $"Se importaron/actualizaron {cantidad} empleados.";
            ResultadoLabel.IsVisible = true;

            await CargarListaAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo importar la nómina: {ex.Message}", "OK");
        }
        finally
        {
            ImportarButton.IsEnabled = true;
            ImportandoIndicator.IsVisible = false;
            ImportandoIndicator.IsRunning = false;
        }
    }

    private async void OnDarDeBajaSwipe(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem || swipeItem.BindingContext is not Empleado empleado)
            return;

        if (!auth.EsAdmin)
        {
            await DisplayAlert("Atención", "Solo un administrador puede dar de baja empleados.", "OK");
            return;
        }

        bool confirmar = await DisplayAlert("Dar de baja", $"¿Dar de baja a {empleado.Nombre}?", "Dar de baja", "Cancelar");

        if (!confirmar)
            return;

        var ok = await supabase.DesactivarEmpleadoAsync(empleado);

        if (!ok)
        {
            await DisplayAlert("Error", "No se pudo dar de baja al empleado.", "OK");
            return;
        }

        lista.Remove(empleado);
    }

    private async void OnEliminarSwipe(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem || swipeItem.BindingContext is not Empleado empleado)
            return;

        if (!auth.EsAdmin)
        {
            await DisplayAlert("Atención", "Solo un administrador puede eliminar empleados.", "OK");
            return;
        }

        bool confirmar = await DisplayAlert("Eliminar", $"¿Eliminar a {empleado.Nombre} (legajo {empleado.Legajo})? Esta acción no se puede deshacer.", "Eliminar", "Cancelar");

        if (!confirmar)
            return;

        var (ok, error) = await supabase.EliminarEmpleadoAsync(empleado);

        if (!ok)
        {
            await DisplayAlert("Error", $"No se pudo eliminar.\n\n{error}", "OK");
            return;
        }

        lista.Remove(empleado);
        todosLosEmpleados.Remove(empleado);
    }

    private static List<(string Legajo, string Nombre)> ParsearExcel(Stream stream)
    {
        var filas = new List<(string, string)>();
        using var workbook = new XLWorkbook(stream);
        var hoja = workbook.Worksheets.First();

        bool esEncabezado = true;

        foreach (var row in hoja.RowsUsed())
        {
            if (esEncabezado)
            {
                esEncabezado = false;
                continue;
            }

            var legajo = row.Cell(1).GetString().Trim();
            var nombre = row.Cell(2).GetString().Trim();

            if (!string.IsNullOrWhiteSpace(legajo) && !string.IsNullOrWhiteSpace(nombre))
                filas.Add((legajo, nombre));
        }

        return filas;
    }

    private static List<(string Legajo, string Nombre)> ParsearCsv(Stream stream)
    {
        var filas = new List<(string, string)>();
        using var reader = new StreamReader(stream);

        bool esEncabezado = true;
        string? linea;

        while ((linea = reader.ReadLine()) is not null)
        {
            if (esEncabezado)
            {
                esEncabezado = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(linea))
                continue;

            var partes = linea.Split(',');

            if (partes.Length < 2)
                continue;

            var legajo = partes[0].Trim().Trim('"');
            var nombre = partes[1].Trim().Trim('"');

            if (!string.IsNullOrWhiteSpace(legajo) && !string.IsNullOrWhiteSpace(nombre))
                filas.Add((legajo, nombre));
        }

        return filas;
    }
}
