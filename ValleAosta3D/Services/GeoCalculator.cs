using ValleAosta3D.Models;

namespace ValleAosta3D.Services;

/// <summary>
/// Provides methods for calculating model dimensions and geographic bounds.
/// </summary>
public static class GeoCalculator
{
    private const double KilometersPerLatitudeDegree = 111.32;

    /// <summary>
    /// Returns the physical width of the model in millimeters from real-world width and scale.
    /// </summary>
    public static double GetModelWidthMm(
        double realWidthKm,
        ModelOptions model)
    {
        return realWidthKm * 1_000_000d / model.Scale;
    }

    /// <summary>
    /// Returns the physical height of the model in millimeters from real-world height and scale.
    /// </summary>
    public static double GetModelHeightMm(
        double realHeightKm,
        ModelOptions model)
    {
        return realHeightKm * 1_000_000d / model.Scale;
    }

    /// <summary>
    /// Returns the real-world width covered by a model width in millimeters.
    /// </summary>
    public static double GetRealWidthKm(
        double modelWidthMm,
        ModelOptions model)
    {
        return modelWidthMm * model.Scale / 1_000_000d;
    }

    /// <summary>
    /// Returns the real-world height covered by a model height in millimeters.
    /// </summary>
    public static double GetRealHeightKm(
        double modelHeightMm,
        ModelOptions model)
    {
        return modelHeightMm * model.Scale / 1_000_000d;
    }

    /// <summary>
    /// Returns the width of a single tile in the real world.
    /// </summary>
    public static double GetTileWidthKm(ModelOptions model)
    {
        return model.TileSizeMm * model.Scale / 1_000_000d;
    }

    /// <summary>
    /// Returns the height of a single tile in the real world.
    /// </summary>
    public static double GetTileHeightKm(ModelOptions model)
    {
        return model.TileSizeMm * model.Scale / 1_000_000d;
    }

    /// <summary>
    /// Calculates the source bounding box for terrain acquisition.
    /// Requires explicit South/West/North/East bounds.
    /// </summary>
    public static BoundingBox CalculateSourceBoundingBox(
        AreaOptions area,
        ModelOptions _)
    {
        if (!area.HasExplicitBoundingBox)
        {
            throw new InvalidOperationException(
                "Automatic tile layout requires explicit Area South/West/North/East bounds.");
        }

        BoundingBox configuredBoundingBox = new(
            South: area.South!.Value,
            West: area.West!.Value,
            North: area.North!.Value,
            East: area.East!.Value);

        ValidateBoundingBox(configuredBoundingBox, nameof(area));

        return configuredBoundingBox;
    }

    /// <summary>
    /// Calculates the padded bounding box to download from DEM providers.
    /// </summary>
    public static BoundingBox CalculateDownloadBoundingBox(
        AreaOptions area,
        ModelOptions model,
        double paddingPercentPerSide)
    {
        BoundingBox sourceBoundingBox = CalculateSourceBoundingBox(area, model);
        return ApplyPadding(sourceBoundingBox, paddingPercentPerSide);
    }

    /// <summary>
    /// Expands the padded box to the minimum centered box representable by an integer tile grid.
    /// </summary>
    public static BoundingBox ExpandToMinimumTileGrid(
        BoundingBox padded,
        ModelOptions model,
        out int tilesX,
        out int tilesY,
        out double snappedWidthKm,
        out double snappedHeightKm)
    {
        double tileWidthKm = GetTileWidthKm(model);
        double tileHeightKm = GetTileHeightKm(model);

        if (tileWidthKm <= 0 || tileHeightKm <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(model),
                "Tile size and scale must produce positive real-world tile dimensions.");
        }

        double paddedWidthKm = GetBoundingBoxWidthKm(padded);
        double paddedHeightKm = GetBoundingBoxHeightKm(padded);

        tilesX = Math.Max(1, (int)Math.Ceiling(paddedWidthKm / tileWidthKm));
        tilesY = Math.Max(1, (int)Math.Ceiling(paddedHeightKm / tileHeightKm));

        snappedWidthKm = tilesX * tileWidthKm;
        snappedHeightKm = tilesY * tileHeightKm;

        return ExpandBoundingBoxToSize(
            padded,
            snappedWidthKm,
            snappedHeightKm);
    }

    /// <summary>
    /// Returns the width of a bounding box in real-world kilometers.
    /// </summary>
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
    public static double GetBoundingBoxHeightKm(BoundingBox boundingBox)
    {
        return (boundingBox.North - boundingBox.South) * KilometersPerLatitudeDegree;
    }

    /// <summary>
    /// Returns the width/height ratio of a bounding box in real-world kilometers.
    /// </summary>
    public static double GetBoundingBoxAspectRatio(BoundingBox boundingBox)
    {
        double heightKm = GetBoundingBoxHeightKm(boundingBox);
        return GetBoundingBoxWidthKm(boundingBox) / heightKm;
    }

    /// <summary>
    /// Applies symmetric per-side padding to a bounding box.
    /// </summary>
    public static BoundingBox ApplyPadding(
        BoundingBox source,
        double paddingPercentPerSide)
    {
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

        return BuildCenteredBoundingBox(
            centerLatitude,
            centerLongitude,
            paddedWidthKm,
            paddedHeightKm);
    }

    /// <summary>
    /// Expands a box around its center to the requested real-world size.
    /// </summary>
    public static BoundingBox ExpandBoundingBoxToSize(
        BoundingBox source,
        double widthKm,
        double heightKm)
    {
        if (widthKm <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(widthKm),
                "Width must be greater than zero.");
        }

        if (heightKm <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(heightKm),
                "Height must be greater than zero.");
        }

        double centerLatitude = (source.South + source.North) / 2.0;
        double centerLongitude = (source.West + source.East) / 2.0;

        return BuildCenteredBoundingBox(
            centerLatitude,
            centerLongitude,
            widthKm,
            heightKm);
    }

    /// <summary>
    /// Returns how many real-world meters are represented by a single horizontal sample.
    /// </summary>
    public static double GetMetersPerSample(ModelOptions model)
    {
        return model.HorizontalResolutionMm *
               model.Scale /
               1000.0;
    }

    /// <summary>
    /// Returns the number of horizontal samples required for the given model width.
    /// </summary>
    public static int GetRequiredWidthSamples(
        double modelWidthMm,
        ModelOptions model)
    {
        if (modelWidthMm <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(modelWidthMm),
                "Model width must be greater than zero.");
        }

        return (int)Math.Ceiling(modelWidthMm / model.HorizontalResolutionMm);
    }

    /// <summary>
    /// Returns the number of horizontal samples required for the given model height.
    /// </summary>
    public static int GetRequiredHeightSamples(
        double modelHeightMm,
        ModelOptions model)
    {
        if (modelHeightMm <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(modelHeightMm),
                "Model height must be greater than zero.");
        }

        return (int)Math.Ceiling(modelHeightMm / model.HorizontalResolutionMm);
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
