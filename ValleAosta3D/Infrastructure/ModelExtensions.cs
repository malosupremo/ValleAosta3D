using ValleAosta3D.Models;

namespace ValleAosta3D.Infrastructure;

/// <summary>
/// Provides extension methods for model parameters.
/// </summary>
public static class ModelExtensions
{
    /// <summary>
    /// Returns the width of the model in millimeters from tile count.
    /// </summary>
    public static int GetWidthMm(
        this ModelOptions model,
        int tilesX)
    {
        return tilesX * model.TileSizeMm;
    }

    /// <summary>
    /// Returns the height of the model in millimeters from tile count.
    /// </summary>
    public static int GetHeightMm(
        this ModelOptions model,
        int tilesY)
    {
        return tilesY * model.TileSizeMm;
    }
}
