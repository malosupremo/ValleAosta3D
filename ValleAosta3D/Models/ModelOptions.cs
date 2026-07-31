namespace ValleAosta3D.Models;

/// <summary>
/// Represents the physical parameters of the 3D model.
/// </summary>
public sealed class ModelOptions
{
    /// <summary>
    /// Model scale factor relative to the real world (eg: 1:400000).
    /// </summary>
    public int Scale { get; set; }

    /// <summary>
    /// Vertical exaggeration multiplier applied to the model.
    /// </summary>
    public int VerticalExaggeration { get; set; }

    /// <summary>
    /// Number of tiles along the X axis.
    /// </summary>
    public int TilesX { get; set; }

    /// <summary>
    /// Number of tiles along the Y axis.
    /// </summary>
    public int TilesY { get; set; }

    /// <summary>
    /// Size of each tile in millimeters.
    /// </summary>
    public int TileSizeMm { get; set; }

    /// <summary>
    /// Desired horizontal resolution of the generated terrain.
    /// </summary>
    public double HorizontalResolutionMm { get; set; }
}
