using ValleAosta3D.Infrastructure;
using ValleAosta3D.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ValleAosta3D.Services;

/// <summary>
/// Provides cache paths for downloaded DEM files.
/// </summary>
/// <remarks>
/// Initializes a new cache service instance.
/// </remarks>
/// <param name="folders">Application folders configuration.</param>
public sealed class DemCacheService(ApplicationFolders folders)
{
    private readonly ApplicationFolders _folders = folders;

    /// <summary>
    /// Returns the expected cache path for a DEM request signature.
    /// </summary>
    /// <param name="demType">DEM dataset type.</param>
    /// <param name="boundingBox">Requested geographic bounding box.</param>
    /// <param name="outputFormat">Requested output format.</param>
    /// <returns>Absolute path of the cached DEM file.</returns>
    public string GetCachedDemPath(
        string demType,
        BoundingBox boundingBox,
        string outputFormat)
    {
        string hash = ComputeRequestHash(demType, boundingBox, outputFormat);
        string extension = GetExtension(outputFormat);
        string safeDemType = SanitizeToken(demType).ToLowerInvariant();

        return Path.Combine(
            _folders.RawCache,
            $"{safeDemType}-{hash}.{extension}");
    }

    /// <summary>
    /// Returns whether a cached file already exists for the specified request.
    /// </summary>
    /// <param name="demType">DEM dataset type.</param>
    /// <param name="boundingBox">Requested geographic bounding box.</param>
    /// <param name="outputFormat">Requested output format.</param>
    /// <returns><c>true</c> if the file is already cached; otherwise <c>false</c>.</returns>
    public bool Exists(
        string demType,
        BoundingBox boundingBox,
        string outputFormat)
    {
        return File.Exists(GetCachedDemPath(demType, boundingBox, outputFormat));
    }

    private static string ComputeRequestHash(
        string demType,
        BoundingBox boundingBox,
        string outputFormat)
    {
        string signature = string.Create(
            CultureInfo.InvariantCulture,
            $"{demType.ToUpperInvariant()}|{outputFormat.ToUpperInvariant()}|{boundingBox.South:F6}|{boundingBox.West:F6}|{boundingBox.North:F6}|{boundingBox.East:F6}");

        byte[] signatureBytes = Encoding.UTF8.GetBytes(signature);
        byte[] hashBytes = SHA256.HashData(signatureBytes);
        string hash = Convert.ToHexString(hashBytes);

        return hash[..16].ToLowerInvariant();
    }

    private static string GetExtension(string outputFormat)
    {
        if (string.Equals(outputFormat, "GTiff", StringComparison.OrdinalIgnoreCase))
        {
            return "tif";
        }

        return SanitizeToken(outputFormat).ToLowerInvariant();
    }

    private static string SanitizeToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        }

        StringBuilder builder = new(value.Length);

        foreach (char c in value)
        {
            if (char.IsLetterOrDigit(c) || c is '-' or '_')
            {
                builder.Append(c);
            }
        }

        if (builder.Length == 0)
        {
            throw new ArgumentException(
                "Value must contain at least one alphanumeric character.",
                nameof(value));
        }

        return builder.ToString();
    }
}