using System.Text.Json;

namespace Circulacion_Barracas;

public class UpdateService
{
    private readonly HttpClient _httpClient = new();

    // CAMBIAR por tu URL REAL del raw file de GitHub
    private const string VersionUrl = "https://raw.githubusercontent.com/ELJotelson/CIRCULAR-BARRACAS-APP/main/version.json";

    public async Task<UpdateInfo?> GetLatestVersionAsync()
    {
        try
        {
            var json = await _httpClient.GetStringAsync(VersionUrl);

            var info = JsonSerializer.Deserialize<UpdateInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return info;
        }
        catch
        {
            return null;
        }
    }

    public bool IsUpdateAvailable(string currentVersion, string latestVersion)
    {
        if (Version.TryParse(currentVersion, out var current) &&
            Version.TryParse(latestVersion, out var latest))
        {
            return latest > current;
        }

        return false;
    }
}