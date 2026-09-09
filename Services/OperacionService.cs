using Circulacion_Barracas.Models;

namespace Circulacion_Barracas.Services;

public class OperacionService
{
    private const string PrefKey = "operacion_actual_id";

    private readonly SupabaseService supabase;

    public Operacion? Actual { get; private set; }
    public List<Operacion> Disponibles { get; private set; } = new();

    public event Action? CambioOperacion;

    public OperacionService(SupabaseService supabase)
    {
        this.supabase = supabase;
    }

    public async Task CargarAsync()
    {
        Disponibles = await supabase.ObtenerOperacionesActivasAsync();

        var idGuardado = Preferences.Default.Get(PrefKey, string.Empty);

        Actual = Disponibles.FirstOrDefault(o => o.Id.ToString() == idGuardado)
            ?? (Disponibles.Count == 1 ? Disponibles[0] : null);

        if (Actual is not null)
            Preferences.Default.Set(PrefKey, Actual.Id.ToString());
    }

    public void Seleccionar(Operacion operacion)
    {
        Actual = operacion;
        Preferences.Default.Set(PrefKey, operacion.Id.ToString());
        CambioOperacion?.Invoke();
    }
}
