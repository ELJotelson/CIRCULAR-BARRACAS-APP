using Circulacion_Barracas.Models;
using SQLite;

namespace Circulacion_Barracas.Services;

public class DatabaseService
{
    private readonly SQLiteConnection _database;

    public DatabaseService(string dbPath)
    {
        _database = new SQLiteConnection(dbPath);
        _database.CreateTable<Desvio>();
        _database.CreateTable<EmpleadoCache>();
    }

    public void GuardarDesvio(Desvio desvio)
    {
        _database.Insert(desvio);
    }

    public List<Desvio> ObtenerDesvios()
    {
        return _database.Table<Desvio>()
                        .OrderByDescending(x => x.Fecha)
                        .ToList();
    }

    public Desvio? ObtenerDesvioPorId(int id)
    {
        return _database.Table<Desvio>()
                        .FirstOrDefault(x => x.Id == id);
    }

    public bool EliminarDesvio(int id)
    {
        try
        {
            var desvio = ObtenerDesvioPorId(id);

            if (desvio == null)
                return false;

            if (!string.IsNullOrEmpty(desvio.FotoPath) && File.Exists(desvio.FotoPath))
            {
                File.Delete(desvio.FotoPath);
            }

            int filas = _database.Delete(desvio);
            return filas > 0;
        }
        catch
        {
            return false;
        }
    }

    public bool EliminarDesvioPorRemoteId(long remoteId)
    {
        var desvio = _database.Table<Desvio>().FirstOrDefault(x => x.RemoteId == remoteId);

        if (desvio == null)
            return false;

        return EliminarDesvio(desvio.Id);
    }

    public void GuardarNominaLocal(List<EmpleadoCache> empleados)
    {
        _database.RunInTransaction(() =>
        {
            _database.DeleteAll<EmpleadoCache>();
            _database.InsertAll(empleados);
        });
    }

    public List<EmpleadoCache> ObtenerNominaLocal()
    {
        return _database.Table<EmpleadoCache>()
                        .OrderBy(x => x.Nombre)
                        .ToList();
    }
}
