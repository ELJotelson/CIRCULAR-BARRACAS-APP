using System.Text.Json;
using Circulacion_Barracas.Models;

namespace Circulacion_Barracas.Services;

public class UpdateService
{
    private readonly HttpClient _httpClient = new();

    private const string VersionUrl = "https://raw.githubusercontent.com/ELJotelson/CIRCULAR-BARRACAS-APP/main/version.json";

    public async Task<UpdateInfo?> GetLatestVersionAsync()
    {
        var json = await _httpClient.GetStringAsync(VersionUrl);

        var info = JsonSerializer.Deserialize<UpdateInfo>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return info;
    }

    public bool IsUpdateAvailable(string currentVersion, string latestVersion)
    {
        string normalizedCurrent = NormalizeVersion(currentVersion);
        string normalizedLatest = NormalizeVersion(latestVersion);

        if (Version.TryParse(normalizedCurrent, out var current) &&
            Version.TryParse(normalizedLatest, out var latest))
        {
            return latest > current;
        }

        return false;
    }

    private string NormalizeVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return "0.0.0";

        var parts = version.Split('.').ToList();

        while (parts.Count < 3)
            parts.Add("0");

        return string.Join(".", parts);
    }
}
