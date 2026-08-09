namespace ValleAosta3D.Models;

/// <summary>
/// Represents metadata for a cached resampled DEM grid.
/// </summary>
public sealed class ResampledDemMetadata
{
    /// <summary>
    /// Cache key that identifies the resampled artifact.
    /// </summary>
    public string CacheKey { get; set; } = "";

    /// <summary>
    /// Source DEM file name used to generate the resampled grid.
    /// </summary>
    public string SourceDemFileName { get; set; } = "";

    /// <summary>
    /// DEM dataset type used for the source file.
    /// </summary>
    public string DemType { get; set; } = "";

    /// <summary>
    /// Bounding box south latitude.
    /// </summary>
    public double South { get; set; }

    /// <summary>
    /// Bounding box west longitude.
    /// </summary>
    public double West { get; set; }

    /// <summary>
    /// Bounding box north latitude.
    /// </summary>
    public double North { get; set; }

    /// <summary>
    /// Bounding box east longitude.
    /// </summary>
    public double East { get; set; }

    /// <summary>
    /// Source width in samples.
    /// </summary>
    public int SourceWidth { get; set; }

    /// <summary>
    /// Source height in samples.
    /// </summary>
    public int SourceHeight { get; set; }

    /// <summary>
    /// Target width in samples.
    /// </summary>
    public int TargetWidth { get; set; }

    /// <summary>
    /// Target height in samples.
    /// </summary>
    public int TargetHeight { get; set; }

    /// <summary>
    /// Horizontal meters represented by each target sample.
    /// </summary>
    public double MetersPerSample { get; set; }
}
