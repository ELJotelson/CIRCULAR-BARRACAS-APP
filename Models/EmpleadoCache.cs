using SQLite;

namespace Circulacion_Barracas.Models;

public class EmpleadoCache
{
    [PrimaryKey]
    public string Legajo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;
}
