using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Circulacion_Barracas.Models;

[Table("empleados")]
public class Empleado : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("operacion_id")]
    public Guid OperacionId { get; set; }

    [Column("legajo")]
    public string Legajo { get; set; } = string.Empty;

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public string DisplayName => $"{Legajo} - {Nombre}";
}
