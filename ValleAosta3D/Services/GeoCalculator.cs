using ValleAosta3D.Models;

namespace ValleAosta3D.Services;

/// <summary>
/// Provides methods for calculating the model's dimensions and geographic bounds.
/// </summary>
public static class GeoCalculator
{
    /// <summary>
    /// Returns the physical width of the model in millimeters.
    /// </summary>
    /// <param name="model">Model parameters.</param>
    /// <returns>The width of the model in millimeters.</returns>
    public static double GetModelWidthMm(ModelOptions model)
    {
        return model.TilesX * model.TileSizeMm;
    }

    /// <summary>
    /// Returns the physical height of the model in millimeters.
    /// </summary>
    /// <param name="model">Model parameters.</param>
    /// <returns>The height of the model in millimeters.</returns>
    public static double GetModelHeightMm(ModelOptions model)
    {
        return model.TilesY * model.TileSizeMm;
    }

    /// <summary>
    /// Returns the width covered by the model in the real world,
    /// expressed in kilometers.
    /// </summary>
    /// <param name="model">Model parameters.</param>
    /// <returns>The real-world width of the model in kilometers.</returns>
    public static double GetRealWidthKm(ModelOptions model)
    {
        return GetModelWidthMm(model) * model.Scale / 1_000_000d;
    }

    /// <summary>
    /// Returns the height covered by the model in the real world,
    /// expressed in kilometers.
    /// </summary>
    /// <param name="model">Model parameters.</param>
    /// <returns>The real-world height of the model in kilometers.</returns>
    public static double GetRealHeightKm(ModelOptions model)
    {
        return GetModelHeightMm(model) * model.Scale / 1_000_000d;
    }

    /// <summary>
    /// Returns the width of a single tile in the real world.
    /// </summary>
    /// <param name="model">Model parameters.</param>
    /// <returns>The width of a tile in kilometers.</returns>
    public static double GetTileWidthKm(ModelOptions model)
    {
        return model.TileSizeMm * model.Scale / 1_000_000d;
    }

    /// <summary>
    /// Returns the height of a single tile in the real world.
    /// </summary>
    /// <param name="model">Model parameters.</param>
    /// <returns>The height of a tile in kilometers.</returns>
    public static double GetTileHeightKm(ModelOptions model)
    {
        return model.TileSizeMm * model.Scale / 1_000_000d;
    }

    /// <summary>
    /// Calculates the geographic rectangle that must be covered by the model,
    /// centered on the coordinates specified in the configuration.
    /// </summary>
    /// <param name="area">
    /// Coordinates of the center of the area to model.
    /// </param>
    /// <param name="model">
    /// Physical parameters of the model (scale, tile count, dimensions).
    /// </param>
    /// <returns>
    /// Bounding box expressed as geographic South/West/North/East coordinates.
    /// </returns>
    public static BoundingBox CalculateBoundingBox(
        AreaOptions area,
        ModelOptions model)
    {
        double widthKm = GetRealWidthKm(model);
        double heightKm = GetRealHeightKm(model);

        double halfWidthKm = widthKm / 2.0;
        double halfHeightKm = heightKm / 2.0;

        double latitudeDegreesPerKm = 1.0 / 111.32;

        double longitudeDegreesPerKm =
            1.0 / (111.32 *
                   Math.Cos(area.CenterLatitude * Math.PI / 180.0));

        double deltaLatitude =
            halfHeightKm * latitudeDegreesPerKm;

        double deltaLongitude =
            halfWidthKm * longitudeDegreesPerKm;

        return new BoundingBox(
            South: area.CenterLatitude - deltaLatitude,
            West: area.CenterLongitude - deltaLongitude,
            North: area.CenterLatitude + deltaLatitude,
            East: area.CenterLongitude + deltaLongitude);
    }

    /// <summary>
    /// Returns how many real-world meters are represented by a single
    /// horizontal sample of the model.
    /// </summary>
    public static double GetMetersPerSample(
        ModelOptions model)
    {
        return model.HorizontalResolutionMm *
               model.Scale /
               1000.0;
    }

    /// <summary>
    /// Returns the number of horizontal samples required
    /// to represent the model width at the configured resolution.
    /// </summary>
    public static int GetRequiredWidthSamples(
        ModelOptions model)
    {
        return (int)Math.Ceiling(
            GetModelWidthMm(model) /
            model.HorizontalResolutionMm);
    }

    /// <summary>
    /// Returns the number of horizontal samples required
    /// to represent the model height at the configured resolution.
    /// </summary>
    public static int GetRequiredHeightSamples(
        ModelOptions model)
    {
        return (int)Math.Ceiling(
            GetModelHeightMm(model) /
            model.HorizontalResolutionMm);
    }
}
