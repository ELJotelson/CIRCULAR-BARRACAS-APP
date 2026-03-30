using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Circulacion_Barracas;

public class SupabaseService
{
    private readonly HttpClient _httpClient;

    private const string SupabaseUrl = "https://achucxnpgcavdhidrtve.supabase.co";
    private const string SupabaseAnonKey = "sb_publishable_WQrItYFK3otIlELPvWLaBA_z28mOPk2";
    private const string Tabla = "desvios";
    private const string Bucket = "Desvios_Fotos";

    public SupabaseService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("apikey", SupabaseAnonKey);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", SupabaseAnonKey);
    }

    public async Task<string?> SubirImagenAsync(string rutaLocalImagen)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rutaLocalImagen) || !File.Exists(rutaLocalImagen))
                return null;

            var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(rutaLocalImagen)}";
            var urlUpload = $"{SupabaseUrl}/storage/v1/object/{Bucket}/{nombreArchivo}";

            byte[] bytes = await File.ReadAllBytesAsync(rutaLocalImagen);

            using var contenido = new ByteArrayContent(bytes);
            contenido.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            var request = new HttpRequestMessage(HttpMethod.Post, urlUpload);
            request.Content = contenido;
            request.Headers.Add("x-upsert", "true");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            return $"{SupabaseUrl}/storage/v1/object/public/{Bucket}/{nombreArchivo}";
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> GuardarDesvioAsync(Desvio desvio)
    {
        try
        {
            string? fotoUrl = null;

            if (!string.IsNullOrWhiteSpace(desvio.FotoPath) && File.Exists(desvio.FotoPath))
            {
                fotoUrl = await SubirImagenAsync(desvio.FotoPath);
            }

            var payload = new
            {
                fecha = desvio.Fecha,
                inspector = desvio.Inspector,
                empleado = desvio.Empleado,
                lugar = desvio.Lugar,
                tipo_desvio = desvio.TipoDesvio,
                observaciones = desvio.Observaciones,
                foto_url = fotoUrl
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{SupabaseUrl}/rest/v1/{Tabla}",
                content
            );

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Desvio>> ObtenerDesviosAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{SupabaseUrl}/rest/v1/{Tabla}?select=*&order=fecha.desc"
            );

            if (!response.IsSuccessStatusCode)
                return new List<Desvio>();

            var json = await response.Content.ReadAsStringAsync();

            var opciones = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var items = JsonSerializer.Deserialize<List<SupabaseDesvioDto>>(json, opciones)
                        ?? new List<SupabaseDesvioDto>();

            return items.Select(x => new Desvio
            {
                Id = x.id,
                Fecha = x.fecha,
                Inspector = x.inspector ?? string.Empty,
                Empleado = x.empleado ?? string.Empty,
                Lugar = x.lugar ?? string.Empty,
                TipoDesvio = x.tipo_desvio ?? string.Empty,
                Observaciones = x.observaciones ?? string.Empty,
                FotoPath = x.foto_url ?? string.Empty
            }).ToList();
        }
        catch
        {
            return new List<Desvio>();
        }
    }

    private class SupabaseDesvioDto
    {
        public int id { get; set; }
        public DateTime fecha { get; set; }
        public string? inspector { get; set; }
        public string? empleado { get; set; }
        public string? lugar { get; set; }
        public string? tipo_desvio { get; set; }
        public string? observaciones { get; set; }
        public string? foto_url { get; set; }
    }
}