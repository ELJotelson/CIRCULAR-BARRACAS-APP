using SQLite;

namespace Circulacion_Barracas.Models;

public class Desvio
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public long? RemoteId { get; set; }
    public string Inspector { get; set; } = string.Empty;
    public string Empleado { get; set; } = string.Empty;
    public string? EmpleadoId { get; set; }
    public string Lugar { get; set; } = string.Empty;
    public string TipoDesvio { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public string FotoPath { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string? UserId { get; set; }
    public string? OperacionId { get; set; }
}
