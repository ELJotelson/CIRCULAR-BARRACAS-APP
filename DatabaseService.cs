using SQLite;

namespace Circulacion_Barracas;

public class DatabaseService
{
    private readonly SQLiteConnection _database;

    public DatabaseService(string dbPath)
    {
        _database = new SQLiteConnection(dbPath);
        _database.CreateTable<Desvio>();
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
}