namespace ValleAosta3D.Models;

/// <summary>
/// Represents the geographic area centered on the specified location.
/// </summary>
public sealed class AreaOptions
{
    /// <summary>
    /// Latitude of the area center.
    /// </summary>
    public double CenterLatitude { get; set; }

    /// <summary>
    /// Longitude of the area center.
    /// </summary>
    public double CenterLongitude { get; set; }
}
