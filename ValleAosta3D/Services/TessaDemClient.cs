using ValleAosta3D.Models;

namespace ValleAosta3D.Services;

public sealed class TessaDemClient(HttpClient httpClient, string apiKey)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _apiKey = apiKey;

    public async Task DownloadAreaAsync(
        BoundingBox bbox,
        int rows,
        int columns,
        string outputFile)
    {
        string url =
            $"https://tessadem.com/api/elevation" +
            $"?key={Uri.EscapeDataString(_apiKey)}" +
            $"&mode=area" +
            $"&rows={rows}" +
            $"&columns={columns}" +
            $"&format=geotiff" +
            $"&locations={bbox.South},{bbox.West}|{bbox.North},{bbox.East}";

        Console.WriteLine(url.Replace(_apiKey, "***"));

        byte[] bytes = await _httpClient.GetByteArrayAsync(url);

        await File.WriteAllBytesAsync(outputFile, bytes);

        Console.WriteLine($"Saved: {outputFile}");
        Console.WriteLine($"Size : {bytes.Length:N0} bytes");
    }
}