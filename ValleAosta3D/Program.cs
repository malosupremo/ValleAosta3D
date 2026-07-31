using Microsoft.Extensions.Configuration;
using ValleAosta3D.Infrastructure;
using ValleAosta3D.Models;
using ValleAosta3D.Services;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

AppOptions options =
    configuration.Get<AppOptions>()
    ?? throw new InvalidOperationException("Invalid configuration");

ApplicationFolders _ = new(options);

Console.WriteLine();
Console.WriteLine("Model");
Console.WriteLine("-----");

Console.WriteLine($"Scale      : 1:{options.Model.Scale:N0}");
Console.WriteLine($"Vertical   : {options.Model.VerticalExaggeration}x");

Console.WriteLine();

Console.WriteLine("Physical size");
Console.WriteLine("-------------");

Console.WriteLine($"Width      : {GeoCalculator.GetModelWidthMm(options.Model):N0} mm");
Console.WriteLine($"Height     : {GeoCalculator.GetModelHeightMm(options.Model):N0} mm");

Console.WriteLine();

Console.WriteLine("Real size");
Console.WriteLine("---------");

Console.WriteLine($"Width      : {GeoCalculator.GetRealWidthKm(options.Model):N1} km");
Console.WriteLine($"Height     : {GeoCalculator.GetRealHeightKm(options.Model):N1} km");

Console.WriteLine();

Console.WriteLine("Tiles");
Console.WriteLine("-----");

Console.WriteLine($"{options.Model.TilesX} x {options.Model.TilesY}");
Console.WriteLine($"{GeoCalculator.GetTileWidthKm(options.Model):N1} km per tile");

BoundingBox bbox =
    GeoCalculator.CalculateBoundingBox(
        options.Area,
        options.Model);

Console.WriteLine();
Console.WriteLine("Bounding box");
Console.WriteLine("------------");

Console.WriteLine($"South : {bbox.South:F6}");
Console.WriteLine($"West  : {bbox.West:F6}");
Console.WriteLine($"North : {bbox.North:F6}");
Console.WriteLine($"East  : {bbox.East:F6}");
