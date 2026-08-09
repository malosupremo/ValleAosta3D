using ValleAosta3D.Models;

namespace ValleAosta3D.Services;

/// <summary>
/// Provides methods for calculating the model's dimensions and geographic bounds.
/// </summary>
public static class GeoCalculator
{
    private const double KilometersPerLatitudeDegree = 111.32;

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
    /// Calculates the geographic rectangle covered by the physical model,
    /// centered on the configured area center.
    /// </summary>
    /// <param name="area">Coordinates of the area center.</param>
    /// <param name="model">Physical parameters of the model.</param>
    /// <returns>Bounding box expressed as geographic South/West/North/East coordinates.</returns>
    public static BoundingBox CalculateBoundingBox(
        AreaOptions area,
        ModelOptions model)
    {
        return BuildCenteredBoundingBox(
            area.CenterLatitude,
            area.CenterLongitude,
            GetRealWidthKm(model),
            GetRealHeightKm(model));
    }

    /// <summary>
    /// Calculates the source bounding box for terrain acquisition.
    /// If explicit bounds are configured in <paramref name="area"/>, those are used.
    /// Otherwise a center-based box is generated from the model size.
    /// </summary>
    /// <param name="area">Area configuration.</param>
    /// <param name="model">Physical parameters of the model.</param>
    /// <returns>Source bounding box expressed as South/West/North/East.</returns>
    public static BoundingBox CalculateSourceBoundingBox(
        AreaOptions area,
        ModelOptions model)
    {
        if (area.HasExplicitBoundingBox)
        {
            BoundingBox configuredBoundingBox = new(
                South: area.South!.Value,
                West: area.West!.Value,
                North: area.North!.Value,
                East: area.East!.Value);

            ValidateBoundingBox(configuredBoundingBox, nameof(area));

            return configuredBoundingBox;
        }

        return CalculateBoundingBox(area, model);
    }

    /// <summary>
    /// Calculates the bounding box to download from DEM providers.
    /// It starts from the source area (explicit bounds if provided, otherwise center-based),
    /// applies per-side padding, then expands only the short side to match
    /// the target tile aspect ratio (<c>TilesX:TilesY</c>).
    /// </summary>
    /// <param name="area">Area configuration.</param>
    /// <param name="model">Physical parameters of the model.</param>
    /// <param name="paddingPercentPerSide">
    /// Padding to add per side (for example <c>0.10</c> means +10% on each side).
    /// </param>
    /// <returns>Adjusted geographic bounding box for DEM download.</returns>
    public static BoundingBox CalculateDownloadBoundingBox(
        AreaOptions area,
        ModelOptions model,
        double paddingPercentPerSide)
    {
        BoundingBox sourceBoundingBox = CalculateSourceBoundingBox(area, model);

        return ApplyPaddingAndAspectRatio(
            sourceBoundingBox,
            model.TilesX,
            model.TilesY,
            paddingPercentPerSide);
    }

    /// <summary>
    /// Returns the width of a bounding box in real-world kilometers.
    /// </summary>
    /// <param name="boundingBox">Bounding box to evaluate.</param>
    /// <returns>Bounding box width in kilometers.</returns>
    public static double GetBoundingBoxWidthKm(BoundingBox boundingBox)
    {
        double centerLatitude = (boundingBox.South + boundingBox.North) / 2.0;
        double longitudeDelta = boundingBox.East - boundingBox.West;

        return longitudeDelta * KilometersPerLatitudeDegree *
               Math.Cos(centerLatitude * Math.PI / 180.0);
    }

    /// <summary>
    /// Returns the height of a bounding box in real-world kilometers.
    /// </summary>
    /// <param name="boundingBox">Bounding box to evaluate.</param>
    /// <returns>Bounding box height in kilometers.</returns>
    public static double GetBoundingBoxHeightKm(BoundingBox boundingBox)
    {
        return (boundingBox.North - boundingBox.South) * KilometersPerLatitudeDegree;
    }

    /// <summary>
    /// Returns the width/height ratio of a bounding box in real-world kilometers.
    /// </summary>
    /// <param name="boundingBox">Bounding box to evaluate.</param>
    /// <returns>The metric aspect ratio (width divided by height).</returns>
    public static double GetBoundingBoxAspectRatio(BoundingBox boundingBox)
    {
        double heightKm = GetBoundingBoxHeightKm(boundingBox);

        return GetBoundingBoxWidthKm(boundingBox) / heightKm;
    }

    /// <summary>
    /// Applies per-side padding to a bounding box and expands only the short side
    /// to match the requested aspect ratio.
    /// </summary>
    /// <param name="source">Source bounding box.</param>
    /// <param name="targetAspectX">Target aspect ratio numerator (width).</param>
    /// <param name="targetAspectY">Target aspect ratio denominator (height).</param>
    /// <param name="paddingPercentPerSide">Padding to add to each side.</param>
    /// <returns>The adjusted bounding box.</returns>
    public static BoundingBox ApplyPaddingAndAspectRatio(
        BoundingBox source,
        int targetAspectX,
        int targetAspectY,
        double paddingPercentPerSide)
    {
        if (targetAspectX <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetAspectX),
                "Aspect X must be greater than zero.");
        }

        if (targetAspectY <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetAspectY),
                "Aspect Y must be greater than zero.");
        }

        if (paddingPercentPerSide < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(paddingPercentPerSide),
                "Padding cannot be negative.");
        }

        double centerLatitude = (source.South + source.North) / 2.0;
        double centerLongitude = (source.West + source.East) / 2.0;

        double paddedWidthKm =
            GetBoundingBoxWidthKm(source) * (1.0 + (paddingPercentPerSide * 2.0));

        double paddedHeightKm =
            GetBoundingBoxHeightKm(source) * (1.0 + (paddingPercentPerSide * 2.0));

        double targetAspect = (double)targetAspectX / targetAspectY;
        double currentAspect = paddedWidthKm / paddedHeightKm;

        if (currentAspect < targetAspect)
        {
            paddedWidthKm = paddedHeightKm * targetAspect;
        }
        else if (currentAspect > targetAspect)
        {
            paddedHeightKm = paddedWidthKm / targetAspect;
        }

        return BuildCenteredBoundingBox(
            centerLatitude,
            centerLongitude,
            paddedWidthKm,
            paddedHeightKm);
    }

    /// <summary>
    /// Returns how many real-world meters are represented by a single
    /// horizontal sample of the model.
    /// </summary>
    /// <param name="model">Model parameters.</param>
    /// <returns>Meters represented by one horizontal sample.</returns>
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
    /// <param name="model">Model parameters.</param>
    /// <returns>Required sample count along the width.</returns>
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
    /// <param name="model">Model parameters.</param>
    /// <returns>Required sample count along the height.</returns>
    public static int GetRequiredHeightSamples(
        ModelOptions model)
    {
        return (int)Math.Ceiling(
            GetModelHeightMm(model) /
            model.HorizontalResolutionMm);
    }

    private static BoundingBox BuildCenteredBoundingBox(
        double centerLatitude,
        double centerLongitude,
        double widthKm,
        double heightKm)
    {
        double halfWidthKm = widthKm / 2.0;
        double halfHeightKm = heightKm / 2.0;

        double latitudeDegreesPerKm = 1.0 / KilometersPerLatitudeDegree;
        double longitudeDegreesPerKm =
            1.0 / (KilometersPerLatitudeDegree *
                   Math.Cos(centerLatitude * Math.PI / 180.0));

        double deltaLatitude = halfHeightKm * latitudeDegreesPerKm;
        double deltaLongitude = halfWidthKm * longitudeDegreesPerKm;

        return new BoundingBox(
            South: centerLatitude - deltaLatitude,
            West: centerLongitude - deltaLongitude,
            North: centerLatitude + deltaLatitude,
            East: centerLongitude + deltaLongitude);
    }

    private static void ValidateBoundingBox(
        BoundingBox boundingBox,
        string paramName)
    {
        if (boundingBox.South >= boundingBox.North)
        {
            throw new ArgumentException(
                "South must be smaller than North.",
                paramName);
        }

        if (boundingBox.West >= boundingBox.East)
        {
            throw new ArgumentException(
                "West must be smaller than East.",
                paramName);
        }
    }
}
