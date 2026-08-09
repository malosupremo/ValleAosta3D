namespace ValleAosta3D.Models;

/// <summary>
/// Represents the geographic area configuration for DEM extraction.
/// </summary>
public sealed class AreaOptions
{
    /// <summary>
    /// Latitude of the area center used when explicit bounds are not provided.
    /// </summary>
    public double CenterLatitude { get; set; }

    /// <summary>
    /// Longitude of the area center used when explicit bounds are not provided.
    /// </summary>
    public double CenterLongitude { get; set; }

    /// <summary>
    /// Southern latitude of an explicit input bounding box.
    /// </summary>
    public double? South { get; set; }

    /// <summary>
    /// Western longitude of an explicit input bounding box.
    /// </summary>
    public double? West { get; set; }

    /// <summary>
    /// Northern latitude of an explicit input bounding box.
    /// </summary>
    public double? North { get; set; }

    /// <summary>
    /// Eastern longitude of an explicit input bounding box.
    /// </summary>
    public double? East { get; set; }

    /// <summary>
    /// Gets a value indicating whether all explicit bounds are configured.
    /// </summary>
    public bool HasExplicitBoundingBox =>
        South.HasValue && West.HasValue && North.HasValue && East.HasValue;

    /// <summary>
    /// Padding to add on each side of the download area.
    /// A value of <c>0.10</c> means 10% per side.
    /// </summary>
    public double DownloadPaddingPercentPerSide { get; set; } = 0.10;
}
