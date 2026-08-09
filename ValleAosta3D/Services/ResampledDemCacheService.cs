using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ValleAosta3D.Infrastructure;
using ValleAosta3D.Models;

namespace ValleAosta3D.Services;

/// <summary>
/// Stores and retrieves resampled DEM grids from cache.
/// </summary>
/// <param name="folders">Application folder paths.</param>
public sealed class ResampledDemCacheService(ApplicationFolders folders)
{
    private readonly ApplicationFolders _folders = folders;

    /// <summary>
    /// Attempts to load a cached resampled DEM grid.
    /// </summary>
    /// <param name="cacheKey">Resampled cache key.</param>
    /// <param name="elevations">Loaded elevation grid on success.</param>
    /// <param name="metadata">Loaded metadata on success.</param>
    /// <returns><c>true</c> if the cache entry exists and was loaded; otherwise <c>false</c>.</returns>
    public bool TryLoad(
        string cacheKey,
        out float[,] elevations,
        out ResampledDemMetadata metadata)
    {
        string dataPath = GetDataPathForKey(cacheKey);
        string metadataPath = GetMetadataPathForKey(cacheKey);

        if (!File.Exists(dataPath) || !File.Exists(metadataPath))
        {
            elevations = new float[0, 0];
            metadata = new ResampledDemMetadata();
            return false;
        }

        string metadataJson = File.ReadAllText(metadataPath);
        metadata = JsonSerializer.Deserialize<ResampledDemMetadata>(metadataJson)
                   ?? throw new InvalidOperationException(
                       $"Invalid metadata JSON in '{metadataPath}'.");

        elevations = ReadGrid(
            dataPath,
            metadata.TargetWidth,
            metadata.TargetHeight);

        return true;
    }

    /// <summary>
    /// Saves a resampled DEM grid to cache as binary float32 plus metadata JSON.
    /// </summary>
    /// <param name="cacheKey">Resampled cache key.</param>
    /// <param name="elevations">Elevation grid to persist.</param>
    /// <param name="metadata">Associated metadata.</param>
    /// <returns>Absolute path of the saved binary grid file.</returns>
    public string Save(
        string cacheKey,
        float[,] elevations,
        ResampledDemMetadata metadata)
    {
        int targetHeight = elevations.GetLength(0);
        int targetWidth = elevations.GetLength(1);

        if (targetWidth != metadata.TargetWidth || targetHeight != metadata.TargetHeight)
        {
            throw new ArgumentException(
                "Metadata dimensions do not match the elevation grid.",
                nameof(metadata));
        }

        string dataPath = GetDataPathForKey(cacheKey);
        string metadataPath = GetMetadataPathForKey(cacheKey);

        WriteGrid(dataPath, elevations);

        string metadataJson = JsonSerializer.Serialize(
            metadata,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(metadataPath, metadataJson);

        return dataPath;
    }

    /// <summary>
    /// Attempts to load only metadata of a cached resampled DEM grid.
    /// </summary>
    /// <param name="cacheKey">Resampled cache key.</param>
    /// <param name="metadata">Loaded metadata on success.</param>
    /// <returns><c>true</c> if metadata exists and was loaded; otherwise <c>false</c>.</returns>
    public bool TryLoadMetadata(
        string cacheKey,
        out ResampledDemMetadata metadata)
    {
        string metadataPath = GetMetadataPathForKey(cacheKey);

        if (!File.Exists(metadataPath))
        {
            metadata = new ResampledDemMetadata();
            return false;
        }

        string metadataJson = File.ReadAllText(metadataPath);
        metadata = JsonSerializer.Deserialize<ResampledDemMetadata>(metadataJson)
                   ?? throw new InvalidOperationException(
                       $"Invalid metadata JSON in '{metadataPath}'.");

        return true;
    }

    /// <summary>
    /// Builds the deterministic cache key for a resampled artifact.
    /// </summary>
    /// <param name="sourceDemPath">Source DEM path.</param>
    /// <param name="downloadBoundingBox">Bounding box used for the DEM request.</param>
    /// <param name="targetWidth">Target width in samples.</param>
    /// <param name="targetHeight">Target height in samples.</param>
    /// <returns>Stable cache key string.</returns>
    public static string BuildCacheKey(
        string sourceDemPath,
        BoundingBox downloadBoundingBox,
        int targetWidth,
        int targetHeight)
    {
        string signature = string.Create(
            CultureInfo.InvariantCulture,
            $"{Path.GetFileName(sourceDemPath)}|{downloadBoundingBox.South:F6}|{downloadBoundingBox.West:F6}|{downloadBoundingBox.North:F6}|{downloadBoundingBox.East:F6}|{targetWidth}|{targetHeight}");

        byte[] signatureBytes = Encoding.UTF8.GetBytes(signature);
        byte[] hashBytes = SHA256.HashData(signatureBytes);
        string hash = Convert.ToHexString(hashBytes)[..16].ToLowerInvariant();

        return $"resampled-{hash}";
    }

    /// <summary>
    /// Returns the absolute path of the cached binary grid file for a cache key.
    /// </summary>
    /// <param name="cacheKey">Resampled cache key.</param>
    /// <returns>Absolute path of the .f32 file.</returns>
    public string GetDataPathForKey(string cacheKey)
    {
        return Path.Combine(
            _folders.ResampledCache,
            $"{cacheKey}.f32");
    }

    /// <summary>
    /// Returns the absolute path of the cached metadata file for a cache key.
    /// </summary>
    /// <param name="cacheKey">Resampled cache key.</param>
    /// <returns>Absolute path of the .json metadata file.</returns>
    public string GetMetadataPathForKey(string cacheKey)
    {
        return Path.Combine(
            _folders.ResampledCache,
            $"{cacheKey}.json");
    }

    private static void WriteGrid(
        string path,
        float[,] elevations)
    {
        using FileStream stream = new(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        using BinaryWriter writer = new(stream);

        int height = elevations.GetLength(0);
        int width = elevations.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                writer.Write(elevations[y, x]);
            }
        }
    }

    private static float[,] ReadGrid(
        string path,
        int width,
        int height)
    {
        float[,] elevations = new float[height, width];

        using FileStream stream = new(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        using BinaryReader reader = new(stream);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                elevations[y, x] = reader.ReadSingle();
            }
        }

        return elevations;
    }
}
