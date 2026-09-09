using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Circulacion_Barracas.Models;

[Table("operaciones")]
public class Operacion : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
