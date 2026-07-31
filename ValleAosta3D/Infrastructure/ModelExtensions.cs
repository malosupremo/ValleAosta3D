using ValleAosta3D.Models;

namespace ValleAosta3D.Infrastructure;

/// <summary>
/// Provides extension methods for model parameters.
/// </summary>
public static class ModelExtensions
{
    /// <summary>
    /// Returns the width of the model in millimeters.
    /// </summary>
    /// <param name="model">Model parameters.</param>
    /// <returns>The width of the model in millimeters.</returns>
    public static int GetWidthMm(this ModelOptions model)
    {
        return model.TilesX * model.TileSizeMm;
    }

    /// <summary>
    /// Returns the height of the model in millimeters.
    /// </summary>
    /// <param name="model">Model parameters.</param>
    /// <returns>The height of the model in millimeters.</returns>
    public static int GetHeightMm(this ModelOptions model)
    {
        return model.TilesY * model.TileSizeMm;
    }
}
