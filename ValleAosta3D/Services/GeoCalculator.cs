using ValleAosta3D.Models;

namespace ValleAosta3D.Services;

public static class GeoCalculator
{
    /// <summary>
    /// Restituisce la larghezza fisica del modello in millimetri.
    /// </summary>
    public static double GetModelWidthMm(ModelOptions model)
    {
        return model.TilesX * model.TileSizeMm;
    }

    public static double GetModelHeightMm(ModelOptions model)
    {
        return model.TilesY * model.TileSizeMm;
    }

    /// <summary>
    /// Restituisce la larghezza coperta dal modello nel mondo reale,
    /// espressa in chilometri.
    /// </summary>
    public static double GetRealWidthKm(ModelOptions model)
    {
        return GetModelWidthMm(model) * model.Scale / 1_000_000d;
    }

    public static double GetRealHeightKm(ModelOptions model)
    {
        return GetModelHeightMm(model) * model.Scale / 1_000_000d;
    }

    public static double GetTileWidthKm(ModelOptions model)
    {
        return model.TileSizeMm * model.Scale / 1_000_000d;
    }

    public static double GetTileHeightKm(ModelOptions model)
    {
        return model.TileSizeMm * model.Scale / 1_000_000d;
    }

    /// <summary>
    /// Calcola il rettangolo geografico che deve essere coperto dal modello,
    /// centrandolo sulle coordinate specificate nella configurazione.
    /// </summary>
    /// <param name="area">
    /// Coordinate del centro dell'area da modellare.
    /// </param>
    /// <param name="model">
    /// Parametri fisici del modello (scala, numero di tessere, dimensioni).
    /// </param>
    /// <returns>
    /// Bounding box espresso come coordinate geografiche
    /// South/West/North/East.
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
}
