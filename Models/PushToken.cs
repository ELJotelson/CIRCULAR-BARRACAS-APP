using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Circulacion_Barracas.Models;

[Table("push_tokens")]
public class PushToken : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("token")]
    public string Token { get; set; } = string.Empty;

    [Column("platform")]
    public string Platform { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
