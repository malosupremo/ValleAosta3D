namespace ValleAosta3D.Models;

/// <summary>
/// Represents options for STL generation from a height map.
/// </summary>
public sealed class StlGenerationOptions
{
    /// <summary>
    /// Horizontal spacing between adjacent samples in millimeters.
    /// </summary>
    public double HorizontalStepMm { get; set; }

    /// <summary>
    /// Conversion factor from elevation meters to model millimeters.
    /// </summary>
    public double ElevationScaleMmPerMeter { get; set; }

    /// <summary>
    /// Minimum base thickness under the terrain in millimeters.
    /// </summary>
    public double BaseThicknessMm { get; set; }
}
