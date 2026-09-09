using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Circulacion_Barracas.Models;

[Table("profiles")]
public class Profile : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("nombre_completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Column("rol")]
    public string Rol { get; set; } = "inspector";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public bool EsAdmin => Rol == "admin";
}
