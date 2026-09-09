using Circulacion_Barracas.Models;

namespace Circulacion_Barracas.Services;

public class AuthService
{
    private readonly SupabaseService supabase;

    public Profile? PerfilActual { get; private set; }

    public AuthService(SupabaseService supabase)
    {
        this.supabase = supabase;
    }

    public bool HaySesion => supabase.Client.Auth.CurrentSession is not null;

    public bool EsAdmin => PerfilActual?.EsAdmin ?? false;

    public Guid? UsuarioActualId =>
        Guid.TryParse(supabase.Client.Auth.CurrentUser?.Id, out var id) ? id : null;

    public string? EmailActual => supabase.Client.Auth.CurrentUser?.Email;

    /// <summary>Nombre a mostrar como "Inspector" en los desvíos que carga el usuario actual.</summary>
    public string NombreMostrado =>
        (!string.IsNullOrWhiteSpace(PerfilActual?.NombreCompleto) ? PerfilActual!.NombreCompleto : EmailActual ?? "Usuario")
        .ToUpperInvariant();

    public async Task<string?> IniciarSesionAsync(string email, string password)
    {
        try
        {
            var session = await supabase.Client.Auth.SignIn(email, password);

            if (session is null)
                return "No se pudo iniciar sesión.";

            await CargarPerfilAsync();
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public async Task<(string? Error, bool RequiereConfirmacion)> RegistrarseAsync(string email, string password, string nombreCompleto)
    {
        try
        {
            var session = await supabase.Client.Auth.SignUp(email, password, new Supabase.Gotrue.SignUpOptions
            {
                Data = new Dictionary<string, object> { { "nombre_completo", nombreCompleto } }
            });

            if (session is null)
                return (null, true);

            await CargarPerfilAsync();
            return (null, false);
        }
        catch (Exception ex)
        {
            return (ex.Message, false);
        }
    }

    public async Task CerrarSesionAsync()
    {
        await supabase.Client.Auth.SignOut();
        PerfilActual = null;
    }

    public async Task<bool> CargarPerfilAsync()
    {
        var userId = UsuarioActualId;

        if (userId is null)
        {
            PerfilActual = null;
            return false;
        }

        try
        {
            // RLS ya limita esto a "mi propia fila" (o todas, si soy admin), así que
            // se filtra en memoria en vez de mandar el uuid en el query string: evita
            // depender de cómo el SDK serializa un Guid dentro de un filtro Where().
            var perfiles = await supabase.Client.From<Profile>().Get();
            PerfilActual = perfiles.Models.FirstOrDefault(p => p.Id == userId.Value);

            return PerfilActual is not null;
        }
        catch
        {
            PerfilActual = null;
            return false;
        }
    }

    public async Task<string?> RecuperarContrasenaAsync(string email)
    {
        try
        {
            var enviado = await supabase.Client.Auth.ResetPasswordForEmail(email);
            return enviado ? null : "No se pudo enviar el email de recuperación.";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
