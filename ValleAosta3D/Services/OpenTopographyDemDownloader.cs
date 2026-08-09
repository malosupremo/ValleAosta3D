using System.Globalization;
using ValleAosta3D.Models;

namespace ValleAosta3D.Services;

/// <summary>
/// Downloads DEM files from OpenTopography and stores them in local cache.
/// </summary>
/// <remarks>
/// Initializes a new downloader instance.
/// </remarks>
/// <param name="cacheService">Cache service for DEM files.</param>
/// <param name="options">OpenTopography API settings.</param>
public sealed class OpenTopographyDemDownloader(
    DemCacheService cacheService,
    OpenTopographyOptions options)
{
    private readonly DemCacheService _cacheService = cacheService;
    private readonly OpenTopographyOptions _options = options;

    /// <summary>
    /// Downloads a DEM for the specified bounding box, or returns an existing cached file.
    /// </summary>
    /// <param name="boundingBox">Geographic area to download.</param>
    /// <param name="cancellationToken">Cancellation token for I/O operations.</param>
    /// <returns>Absolute path of the DEM GeoTIFF file.</returns>
    public async Task<string> DownloadAsync(
        BoundingBox boundingBox,
        CancellationToken cancellationToken = default)
    {
        string cachedPath = _cacheService.GetCachedDemPath(
            _options.DemType,
            boundingBox,
            _options.OutputFormat);

        if (File.Exists(cachedPath))
        {
            Console.WriteLine($"DEM cache hit: {Path.GetFileName(cachedPath)}");
            return cachedPath;
        }

        EnsureConfigurationIsValid();

        Uri requestUri = BuildRequestUri(boundingBox);

        Console.WriteLine($"Downloading DEM: {requestUri}");

        using HttpClient httpClient = new();
        using HttpResponseMessage response = await httpClient.GetAsync(
            requestUri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using Stream sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        string tempPath = $"{cachedPath}.tmp";
        await using (FileStream destinationStream = new(
            tempPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None))
        {
            await CopyWithProgressAsync(
                sourceStream,
                destinationStream,
                response.Content.Headers.ContentLength,
                cancellationToken);
            await destinationStream.FlushAsync(cancellationToken);
        }

        File.Move(tempPath, cachedPath, overwrite: true);

        Console.WriteLine($"DEM cached: {Path.GetFileName(cachedPath)}");

        return cachedPath;
    }

    private void EnsureConfigurationIsValid()
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("OpenTopography base URL is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.DemType))
        {
            throw new InvalidOperationException("OpenTopography DEM type is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.OutputFormat))
        {
            throw new InvalidOperationException("OpenTopography output format is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "OpenTopography API key is missing. Configure 'OpenTopography:ApiKey' via user secrets.");
        }
    }

    private Uri BuildRequestUri(BoundingBox boundingBox)
    {
        string baseUrl = _options.BaseUrl.TrimEnd('/');

        string query = string.Create(
            CultureInfo.InvariantCulture,
            $"demtype={Uri.EscapeDataString(_options.DemType)}" +
            $"&south={boundingBox.South:F6}" +
            $"&north={boundingBox.North:F6}" +
            $"&west={boundingBox.West:F6}" +
            $"&east={boundingBox.East:F6}" +
            $"&outputFormat={Uri.EscapeDataString(_options.OutputFormat)}" +
            $"&API_Key={Uri.EscapeDataString(_options.ApiKey)}");

        return new Uri($"{baseUrl}/API/globaldem?{query}", UriKind.Absolute);
    }

    private static async Task CopyWithProgressAsync(
        Stream sourceStream,
        Stream destinationStream,
        long? totalBytes,
        CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[1024 * 64];
        long totalRead = 0;
        DateTime lastUpdateUtc = DateTime.UtcNow;
        TimeSpan logInterval = TimeSpan.FromMilliseconds(500);

        while (true)
        {
            int read = await sourceStream.ReadAsync(buffer, cancellationToken);

            if (read == 0)
            {
                break;
            }

            await destinationStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            totalRead += read;

            DateTime nowUtc = DateTime.UtcNow;
            bool shouldPrint = (nowUtc - lastUpdateUtc) >= logInterval;

            if (!shouldPrint)
            {
                continue;
            }

            PrintProgress(totalRead, totalBytes);
            Console.WriteLine();
            lastUpdateUtc = nowUtc;
        }

        PrintProgress(totalRead, totalBytes);
    }

    private static void PrintProgress(long downloadedBytes, long? totalBytes)
    {
        if (totalBytes is > 0)
        {
            double percent = downloadedBytes * 100d / totalBytes.Value;
            Console.Write(
                $"\rDownload progress: {percent,6:0.0}% ({FormatBytes(downloadedBytes)} / {FormatBytes(totalBytes.Value)})");
            return;
        }

        Console.Write($"\rDownloaded: {FormatBytes(downloadedBytes)}");
    }

    private static string FormatBytes(long bytes)
    {
        const double oneKb = 1024d;
        const double oneMb = oneKb * 1024d;

        if (bytes >= oneMb)
        {
            return $"{bytes / oneMb:0.00} MB";
        }

        if (bytes >= oneKb)
        {
            return $"{bytes / oneKb:0.00} KB";
        }

        return $"{bytes} B";
    }
}
