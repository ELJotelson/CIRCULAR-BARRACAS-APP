using System.Collections.ObjectModel;
using ClosedXML.Excel;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Circulacion_Barracas;

public partial class ListaDesviosPage : ContentPage
{
    private readonly DatabaseService db;
    private readonly ObservableCollection<Desvio> lista = new();

    public ListaDesviosPage()
    {
        InitializeComponent();

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "desvios.db3");
        db = new DatabaseService(dbPath);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarEmpleados();
        CargarDesvios();
    }

    private void CargarDesvios(string? empleadoFiltro = null)
    {
        lista.Clear();

        var desvios = db.ObtenerDesvios();

        if (!string.IsNullOrWhiteSpace(empleadoFiltro) && empleadoFiltro != "Todos")
        {
            desvios = desvios
                .Where(d => d.Empleado == empleadoFiltro)
                .ToList();
        }

        foreach (var d in desvios)
        {
            lista.Add(d);
        }

        ListaDesvios.ItemsSource = lista;
    }

    private void CargarEmpleados()
    {
        var empleados = db.ObtenerDesvios()
                          .Select(d => d.Empleado)
                          .Where(e => !string.IsNullOrWhiteSpace(e))
                          .Distinct()
                          .OrderBy(e => e)
                          .ToList();

        empleados.Insert(0, "Todos");

        EmpleadoPicker.ItemsSource = empleados;
        EmpleadoPicker.SelectedIndex = 0;
    }

    private void OnEmpleadoFiltroChanged(object sender, EventArgs e)
    {
        var seleccionado = EmpleadoPicker.SelectedItem?.ToString();

        if (string.IsNullOrWhiteSpace(seleccionado) || seleccionado == "Todos")
            CargarDesvios();
        else
            CargarDesvios(seleccionado);
    }

    private async void OnEliminarSwipe(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem)
            return;

        if (swipeItem.BindingContext is not Desvio desvio)
            return;

        bool confirmar = await DisplayAlert(
            "Confirmar",
            $"¿Eliminar el desvío de {desvio.Empleado}?",
            "Sí",
            "No");

        if (!confirmar)
            return;

        bool eliminado = db.EliminarDesvio(desvio.Id);

        if (eliminado)
        {
            lista.Remove(desvio);
            await DisplayAlert("OK", "Desvío eliminado correctamente", "OK");
        }
        else
        {
            await DisplayAlert("Error", $"No se pudo borrar el desvío con ID {desvio.Id}", "OK");
        }
    }

    private async void OnExportarExcelClicked(object sender, EventArgs e)
    {
        try
        {
            var desvios = db.ObtenerDesvios();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Desvios");

            ws.Cell(1, 1).Value = "Inspector";
            ws.Cell(1, 2).Value = "Empleado";
            ws.Cell(1, 3).Value = "Lugar";
            ws.Cell(1, 4).Value = "Tipo de Desvío";
            ws.Cell(1, 5).Value = "Observaciones";
            ws.Cell(1, 6).Value = "Fecha";

            int fila = 2;

            foreach (var d in desvios)
            {
                ws.Cell(fila, 1).Value = d.Inspector;
                ws.Cell(fila, 2).Value = d.Empleado;
                ws.Cell(fila, 3).Value = d.Lugar;
                ws.Cell(fila, 4).Value = d.TipoDesvio;
                ws.Cell(fila, 5).Value = d.Observaciones;
                ws.Cell(fila, 6).Value = d.Fecha.ToString("dd/MM/yyyy HH:mm");
                fila++;
            }

            ws.Columns().AdjustToContents();

            string fileName = $"Desvios_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            string path = Path.Combine(FileSystem.CacheDirectory, fileName);

            workbook.SaveAs(path);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Exportar Desvíos",
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}