using Circulacion_Barracas.Models;
using Supabase.Postgrest;
using Supabase.Realtime.PostgresChanges;
using RealtimeChannel = Supabase.Realtime.RealtimeChannel;
using Constants = Supabase.Postgrest.Constants;

namespace Circulacion_Barracas.Services;

public class SupabaseService
{
    private const string SupabaseUrl = "https://achucxnpgcavdhidrtve.supabase.co";
    private const string SupabaseAnonKey = "sb_publishable_WQrItYFK3otIlELPvWLaBA_z28mOPk2";
    private const string Bucket = "Desvios_Fotos";

    public Supabase.Client Client { get; }

    private bool inicializado;

    public SupabaseService()
    {
        var options = new Supabase.SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false,
            SessionHandler = new SessionPersistence()
        };

        Client = new Supabase.Client(SupabaseUrl, SupabaseAnonKey, options);
    }

    public async Task InitializeAsync()
    {
        if (inicializado)
            return;

        await Client.InitializeAsync();
        inicializado = true;
    }

    public async Task<bool> GuardarDesvioAsync(Desvio desvio)
    {
        try
        {
            string? fotoUrl = null;

            if (!string.IsNullOrWhiteSpace(desvio.FotoPath) && File.Exists(desvio.FotoPath))
            {
                var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(desvio.FotoPath)}";
                await Client.Storage.From(Bucket).Upload(desvio.FotoPath, nombreArchivo);
                fotoUrl = Client.Storage.From(Bucket).GetPublicUrl(nombreArchivo);
            }

            var remoto = new DesvioRemoto
            {
                Fecha = desvio.Fecha,
                Inspector = desvio.Inspector,
                Empleado = desvio.Empleado,
                EmpleadoId = string.IsNullOrWhiteSpace(desvio.EmpleadoId) ? null : Guid.Parse(desvio.EmpleadoId),
                Lugar = desvio.Lugar,
                TipoDesvio = desvio.TipoDesvio,
                Observaciones = desvio.Observaciones,
                FotoUrl = fotoUrl,
                UserId = ParseUserId(Client.Auth.CurrentUser?.Id),
                OperacionId = string.IsNullOrWhiteSpace(desvio.OperacionId) ? null : Guid.Parse(desvio.OperacionId)
            };

            await Client.From<DesvioRemoto>().Insert(remoto);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Desvio>> ObtenerDesviosAsync(Guid operacionId)
    {
        try
        {
            var response = await Client.From<DesvioRemoto>()
                .Where(x => x.OperacionId == operacionId)
                .Order(x => x.Fecha, Constants.Ordering.Descending)
                .Get();

            return response.Models.Select(MapearADesvio).ToList();
        }
        catch
        {
            return new List<Desvio>();
        }
    }

    public async Task<List<Desvio>> ObtenerUltimosDesviosAsync(Guid operacionId, int cantidad)
    {
        try
        {
            var response = await Client.From<DesvioRemoto>()
                .Where(x => x.OperacionId == operacionId)
                .Order(x => x.Fecha, Constants.Ordering.Descending)
                .Limit(cantidad)
                .Get();

            return response.Models.Select(MapearADesvio).ToList();
        }
        catch
        {
            return new List<Desvio>();
        }
    }

    public async Task<RealtimeChannel> SuscribirseANuevosDesviosAsync(Action<Desvio> alRecibirNuevo)
    {
        return await Client.From<DesvioRemoto>().On(PostgresChangesOptions.ListenType.Inserts, (_, change) =>
        {
            var nuevo = change.Model<DesvioRemoto>();

            if (nuevo is not null)
                alRecibirNuevo(MapearADesvio(nuevo));
        });
    }

    public async Task<bool> EliminarDesvioAsync(long remoteId)
    {
        try
        {
            await Client.From<DesvioRemoto>().Where(x => x.Id == remoteId).Delete();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Empleado>> ObtenerEmpleadosActivosAsync(Guid operacionId)
    {
        try
        {
            var response = await Client.From<Empleado>()
                .Where(x => x.OperacionId == operacionId && x.Activo == true)
                .Order(x => x.Nombre, Constants.Ordering.Ascending)
                .Get();

            return response.Models;
        }
        catch
        {
            return new List<Empleado>();
        }
    }

    public async Task<List<Empleado>> ObtenerEmpleadosAsync(Guid operacionId)
    {
        try
        {
            var response = await Client.From<Empleado>()
                .Where(x => x.OperacionId == operacionId)
                .Order(x => x.Nombre, Constants.Ordering.Ascending)
                .Get();

            return response.Models;
        }
        catch
        {
            return new List<Empleado>();
        }
    }

    public async Task<int> ImportarNominaAsync(List<Empleado> empleados)
    {
        var options = new QueryOptions { OnConflict = "operacion_id,legajo" };
        var response = await Client.From<Empleado>().Upsert(empleados, options);
        return response.Models.Count;
    }

    public async Task<bool> DesactivarEmpleadoAsync(Empleado empleado)
    {
        try
        {
            empleado.Activo = false;
            await Client.From<Empleado>().Update(empleado);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Operacion>> ObtenerOperacionesActivasAsync()
    {
        try
        {
            var response = await Client.From<Operacion>()
                .Where(x => x.Activo == true)
                .Order(x => x.Nombre, Constants.Ordering.Ascending)
                .Get();

            return response.Models;
        }
        catch
        {
            return new List<Operacion>();
        }
    }

    public async Task<bool> CrearOperacionAsync(string nombre)
    {
        try
        {
            await Client.From<Operacion>().Insert(new Operacion { Nombre = nombre, Activo = true });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Lugar>> ObtenerLugaresActivosAsync(Guid operacionId)
    {
        try
        {
            var response = await Client.From<Lugar>()
                .Where(x => x.OperacionId == operacionId && x.Activo == true)
                .Order(x => x.Nombre, Constants.Ordering.Ascending)
                .Get();

            return response.Models;
        }
        catch
        {
            return new List<Lugar>();
        }
    }

    public async Task<bool> AgregarLugarAsync(Guid operacionId, string nombre)
    {
        try
        {
            await Client.From<Lugar>().Insert(new Lugar { OperacionId = operacionId, Nombre = nombre, Activo = true });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DesactivarLugarAsync(Lugar lugar)
    {
        try
        {
            lugar.Activo = false;
            await Client.From<Lugar>().Update(lugar);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<TipoDesvio>> ObtenerTiposDesvioActivosAsync(Guid operacionId)
    {
        try
        {
            var response = await Client.From<TipoDesvio>()
                .Where(x => x.OperacionId == operacionId && x.Activo == true)
                .Order(x => x.Nombre, Constants.Ordering.Ascending)
                .Get();

            return response.Models;
        }
        catch
        {
            return new List<TipoDesvio>();
        }
    }

    public async Task<bool> AgregarTipoDesvioAsync(Guid operacionId, string nombre)
    {
        try
        {
            await Client.From<TipoDesvio>().Insert(new TipoDesvio { OperacionId = operacionId, Nombre = nombre, Activo = true });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DesactivarTipoDesvioAsync(TipoDesvio tipo)
    {
        try
        {
            tipo.Activo = false;
            await Client.From<TipoDesvio>().Update(tipo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Profile>> ObtenerPerfilesAsync()
    {
        try
        {
            var response = await Client.From<Profile>()
                .Order(x => x.NombreCompleto, Constants.Ordering.Ascending)
                .Get();

            return response.Models;
        }
        catch
        {
            return new List<Profile>();
        }
    }

    public async Task<bool> CambiarRolAsync(Profile perfil, string nuevoRol)
    {
        try
        {
            perfil.Rol = nuevoRol;
            await Client.From<Profile>().Update(perfil);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static Desvio MapearADesvio(DesvioRemoto r) => new()
    {
        RemoteId = r.Id,
        Fecha = r.Fecha,
        Inspector = r.Inspector ?? string.Empty,
        Empleado = r.Empleado ?? string.Empty,
        EmpleadoId = r.EmpleadoId?.ToString(),
        Lugar = r.Lugar ?? string.Empty,
        TipoDesvio = r.TipoDesvio ?? string.Empty,
        Observaciones = r.Observaciones ?? string.Empty,
        FotoPath = r.FotoUrl ?? string.Empty,
        UserId = r.UserId?.ToString(),
        OperacionId = r.OperacionId?.ToString()
    };

    private static Guid? ParseUserId(string? id) =>
        Guid.TryParse(id, out var guid) ? guid : null;
}
