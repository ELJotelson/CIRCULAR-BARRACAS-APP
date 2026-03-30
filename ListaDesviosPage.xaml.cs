using System.Collections.ObjectModel;
using ClosedXML.Excel;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Circulacion_Barracas;

public partial class ListaDesviosPage : ContentPage
{
    private readonly DatabaseService db;
    private readonly SupabaseService supabase;
    private readonly ObservableCollection<Desvio> lista = new();

    public ListaDesviosPage()
    {
        InitializeComponent();

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "desvios.db3");
        db = new DatabaseService(dbPath);
        supabase = new SupabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarDesdeNubeAsync();
    }

    private async Task CargarDesdeNubeAsync(string? empleadoFiltro = null)
    {
        lista.Clear();

        var desvios = await supabase.ObtenerDesviosAsync();

        if (!string.IsNullOrWhiteSpace(empleadoFiltro) && empleadoFiltro != "Todos")
        {
            desvios = desvios
                .Where(d => d.Empleado == empleadoFiltro)
                .ToList();
        }

        foreach (var d in desvios)
            lista.Add(d);

        ListaDesvios.ItemsSource = lista;

        var empleados = desvios
            .Select(d => d.Empleado)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Distinct()
            .OrderBy(e => e)
            .ToList();

        empleados.Insert(0, "Todos");

        EmpleadoPicker.ItemsSource = empleados;

        if (EmpleadoPicker.SelectedIndex < 0)
            EmpleadoPicker.SelectedIndex = 0;
    }

    private async void OnEmpleadoFiltroChanged(object sender, EventArgs e)
    {
        var seleccionado = EmpleadoPicker.SelectedItem?.ToString();

        if (string.IsNullOrWhiteSpace(seleccionado) || seleccionado == "Todos")
            await CargarDesdeNubeAsync();
        else
            await CargarDesdeNubeAsync(seleccionado);
    }

    private async void OnEliminarSwipe(object sender, EventArgs e)
    {
        await DisplayAlert("Info", "Por ahora el borrado sigue solo local. Después lo conectamos a Supabase.", "OK");
    }

    private async void OnImageTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Image image)
            return;

        if (image.BindingContext is not Desvio desvio)
            return;

        if (string.IsNullOrWhiteSpace(desvio.FotoPath))
        {
            await DisplayAlert("Imagen", "No se encontró la imagen.", "OK");
            return;
        }

        await Navigation.PushModalAsync(new ImageViewerPage(desvio.FotoPath));
    }

    private async void OnExportarExcelClicked(object sender, EventArgs e)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Desvios");

            worksheet.Cell(1, 1).Value = "Fecha";
            worksheet.Cell(1, 2).Value = "Inspector";
            worksheet.Cell(1, 3).Value = "Empleado";
            worksheet.Cell(1, 4).Value = "Lugar";
            worksheet.Cell(1, 5).Value = "Tipo de Desvío";
            worksheet.Cell(1, 6).Value = "Observaciones";
            worksheet.Cell(1, 7).Value = "Foto URL";

            for (int i = 0; i < lista.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = lista[i].Fecha.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(i + 2, 2).Value = lista[i].Inspector;
                worksheet.Cell(i + 2, 3).Value = lista[i].Empleado;
                worksheet.Cell(i + 2, 4).Value = lista[i].Lugar;
                worksheet.Cell(i + 2, 5).Value = lista[i].TipoDesvio;
                worksheet.Cell(i + 2, 6).Value = lista[i].Observaciones;
                worksheet.Cell(i + 2, 7).Value = lista[i].FotoPath;
            }

            string filePath = Path.Combine(FileSystem.CacheDirectory, "Desvios.xlsx");
            workbook.SaveAs(filePath);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Compartir Excel",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo exportar a Excel: {ex.Message}", "OK");
        }
    }
}