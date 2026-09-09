using System.Text.Json;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace Circulacion_Barracas.Services;

public class SessionPersistence : IGotrueSessionPersistence<Session>
{
    private static readonly string SessionFilePath =
        Path.Combine(FileSystem.AppDataDirectory, "session.json");

    public void SaveSession(Session session)
    {
        try
        {
            File.WriteAllText(SessionFilePath, JsonSerializer.Serialize(session));
        }
        catch
        {
            // Si no se puede persistir, el usuario simplemente tendrá que loguearse de nuevo.
        }
    }

    public void DestroySession()
    {
        try
        {
            if (File.Exists(SessionFilePath))
                File.Delete(SessionFilePath);
        }
        catch
        {
        }
    }

    public Session? LoadSession()
    {
        try
        {
            if (!File.Exists(SessionFilePath))
                return null;

            var json = File.ReadAllText(SessionFilePath);
            return JsonSerializer.Deserialize<Session>(json);
        }
        catch
        {
            return null;
        }
    }
}
