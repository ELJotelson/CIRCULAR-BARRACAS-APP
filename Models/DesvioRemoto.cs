using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Circulacion_Barracas.Models;

[Table("desvios")]
public class DesvioRemoto : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("fecha")]
    public DateTime Fecha { get; set; }

    [Column("inspector")]
    public string? Inspector { get; set; }

    [Column("empleado")]
    public string? Empleado { get; set; }

    [Column("empleado_id")]
    public Guid? EmpleadoId { get; set; }

    [Column("lugar")]
    public string? Lugar { get; set; }

    [Column("tipo_desvio")]
    public string? TipoDesvio { get; set; }

    [Column("observaciones")]
    public string? Observaciones { get; set; }

    [Column("foto_url")]
    public string? FotoUrl { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("operacion_id")]
    public Guid? OperacionId { get; set; }
}
