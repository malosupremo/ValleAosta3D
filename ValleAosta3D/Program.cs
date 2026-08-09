using Microsoft.Extensions.Configuration;
using ValleAosta3D.Infrastructure;
using ValleAosta3D.Models;
using ValleAosta3D.Services;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>(optional: true)
    .Build();

AppOptions options =
    configuration.Get<AppOptions>()
    ?? throw new InvalidOperationException("Invalid configuration");

if (string.IsNullOrWhiteSpace(options.OpenTopography.ApiKey))
{
    throw new InvalidOperationException(
        "Missing OpenTopography API key. Set 'OpenTopography:ApiKey' via user secrets.");
}

ApplicationFolders folders = new(options);

Console.WriteLine();
Console.WriteLine("Model");
Console.WriteLine("-----");

Console.WriteLine($"Scale      : 1:{options.Model.Scale:N0}");
Console.WriteLine($"Vertical   : {options.Model.VerticalExaggeration}x");
Console.WriteLine($"Base       : {options.Model.BaseThicknessMm:N1} mm");

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

BoundingBox sourceBbox =
    GeoCalculator.CalculateSourceBoundingBox(
        options.Area,
        options.Model);

BoundingBox downloadBbox =
    GeoCalculator.CalculateDownloadBoundingBox(
        options.Area,
        options.Model,
        options.Area.DownloadPaddingPercentPerSide);

Console.WriteLine();
Console.WriteLine("Source bounding box");
Console.WriteLine("-------------------");
Console.WriteLine(
    $"Input source : {(options.Area.HasExplicitBoundingBox ? "Extremes" : "Center + model size")}");

Console.WriteLine($"South : {sourceBbox.South:F6}");
Console.WriteLine($"West  : {sourceBbox.West:F6}");
Console.WriteLine($"North : {sourceBbox.North:F6}");
Console.WriteLine($"East  : {sourceBbox.East:F6}");
Console.WriteLine(
    $"Size  : {GeoCalculator.GetBoundingBoxWidthKm(sourceBbox):N1} km x {GeoCalculator.GetBoundingBoxHeightKm(sourceBbox):N1} km");
Console.WriteLine(
    $"Ratio : {GeoCalculator.GetBoundingBoxAspectRatio(sourceBbox):F3}");

Console.WriteLine();
Console.WriteLine("Download bounding box");
Console.WriteLine("---------------------");

Console.WriteLine($"South : {downloadBbox.South:F6}");
Console.WriteLine($"West  : {downloadBbox.West:F6}");
Console.WriteLine($"North : {downloadBbox.North:F6}");
Console.WriteLine($"East  : {downloadBbox.East:F6}");
Console.WriteLine(
    $"Size  : {GeoCalculator.GetBoundingBoxWidthKm(downloadBbox):N1} km x {GeoCalculator.GetBoundingBoxHeightKm(downloadBbox):N1} km");
Console.WriteLine(
    $"Ratio : {GeoCalculator.GetBoundingBoxAspectRatio(downloadBbox):F3}");

Console.WriteLine();
Console.WriteLine("Sampling");
Console.WriteLine("--------");

Console.WriteLine(
    $"Horizontal resolution : {options.Model.HorizontalResolutionMm} mm");

Console.WriteLine(
    $"Meters per sample     : {GeoCalculator.GetMetersPerSample(options.Model):N1} m");

Console.WriteLine(
    $"Width samples         : {GeoCalculator.GetRequiredWidthSamples(options.Model):N0}");

Console.WriteLine(
    $"Height samples        : {GeoCalculator.GetRequiredHeightSamples(options.Model):N0}");

DemCacheService cacheService = new(folders);
OpenTopographyDemDownloader demDownloader = new(
    cacheService,
    options.OpenTopography);

string tiffPath = await demDownloader.DownloadAsync(downloadBbox);

GeoTiffInspector.PrintInfo(tiffPath);
GeoTiffInspector.PrintStatistics(tiffPath);

int targetWidth = GeoCalculator.GetRequiredWidthSamples(options.Model);
int targetHeight = GeoCalculator.GetRequiredHeightSamples(options.Model);

ResampledDemCacheService resampledCacheService = new(folders);
string resampledCacheKey = ResampledDemCacheService.BuildCacheKey(
    tiffPath,
    downloadBbox,
    targetWidth,
    targetHeight);

ResampledDemMetadata resampledMetadata;

if (resampledCacheService.TryLoad(
        resampledCacheKey,
        out _,
        out ResampledDemMetadata loadedMetadata))
{
    resampledMetadata = loadedMetadata;

    Console.WriteLine();
    Console.WriteLine(
        $"Resampled cache hit: {resampledMetadata.CacheKey}.f32 ({resampledMetadata.TargetWidth} x {resampledMetadata.TargetHeight})");
}
else
{
    Console.WriteLine();
    Console.WriteLine("Loading full DEM grid...");

    GeoTiffReader geoTiffReader = new();
    float[,] sourceElevations = geoTiffReader.ReadElevations(tiffPath);

    int sourceHeight = sourceElevations.GetLength(0);
    int sourceWidth = sourceElevations.GetLength(1);

    Console.WriteLine();
    Console.WriteLine($"Resampling DEM to {targetWidth} x {targetHeight}...");
    float[,] resampledElevations = HeightMapResampler.ResizeBilinear(
        sourceElevations,
        targetWidth,
        targetHeight);

    resampledMetadata = new ResampledDemMetadata
    {
        CacheKey = resampledCacheKey,
        SourceDemFileName = Path.GetFileName(tiffPath),
        DemType = options.OpenTopography.DemType,
        South = downloadBbox.South,
        West = downloadBbox.West,
        North = downloadBbox.North,
        East = downloadBbox.East,
        SourceWidth = sourceWidth,
        SourceHeight = sourceHeight,
        TargetWidth = targetWidth,
        TargetHeight = targetHeight,
        MetersPerSample = GeoCalculator.GetMetersPerSample(options.Model)
    };

    string resampledPath = resampledCacheService.Save(
        resampledCacheKey,
        resampledElevations,
        resampledMetadata);

    Console.WriteLine($"Resampled grid cached: {resampledPath}");
}

string resampledDataPath = resampledCacheService.GetDataPathForKey(resampledCacheKey);

Console.WriteLine();
Console.Write("Generate STL tiles? [y/N]: ");
string? stlAnswer = Console.ReadLine();

if (string.Equals(stlAnswer, "y", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(stlAnswer, "yes", StringComparison.OrdinalIgnoreCase))
{
    if (resampledMetadata.TargetWidth % options.Model.TilesX != 0 ||
        resampledMetadata.TargetHeight % options.Model.TilesY != 0)
    {
        throw new InvalidOperationException(
            "Resampled grid size is not divisible by the configured tile layout.");
    }

    int tileWidthSamples = resampledMetadata.TargetWidth / options.Model.TilesX;
    int tileHeightSamples = resampledMetadata.TargetHeight / options.Model.TilesY;

    double elevationScaleMmPerMeter =
        (1000.0 / options.Model.Scale) * options.Model.VerticalExaggeration;

    double tileHorizontalStepMm = options.Model.TileSizeMm / (tileWidthSamples - 1d);

    StlGenerationOptions stlOptions = new()
    {
        HorizontalStepMm = tileHorizontalStepMm,
        ElevationScaleMmPerMeter = elevationScaleMmPerMeter,
        BaseThicknessMm = options.Model.BaseThicknessMm
    };

    ResampledDemWindowReader windowReader = new();
    StlGenerator stlGenerator = new();
    string stlFolder = Path.Combine(folders.Output, "Stl");

    for (int tileYFromBottom = 0; tileYFromBottom < options.Model.TilesY; tileYFromBottom++)
    {
        for (int tileX = 0; tileX < options.Model.TilesX; tileX++)
        {
            int sourceTileY = (options.Model.TilesY - 1) - tileYFromBottom;
            int startX = tileX * tileWidthSamples;
            int startY = sourceTileY * tileHeightSamples;

            float[,] tileElevations = windowReader.ReadWindow(
                resampledDataPath,
                resampledMetadata.TargetWidth,
                resampledMetadata.TargetHeight,
                startX,
                startY,
                tileWidthSamples,
                tileHeightSamples);

            int tileLabelX = tileX + 1;
            int tileLabelY = tileYFromBottom + 1;
            string stlPath = Path.Combine(stlFolder, $"tile-{tileLabelX}_{tileLabelY}.stl");

            int triangleCount = stlGenerator.GenerateBinaryStl(
                tileElevations,
                stlPath,
                stlOptions);

            Console.WriteLine($"Saved STL: {stlPath} ({triangleCount:N0} triangles)");
        }
    }
}

Console.WriteLine();
Console.Write("Generate preview PNG? [y/N]: ");
string? previewAnswer = Console.ReadLine();

if (string.Equals(previewAnswer, "y", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(previewAnswer, "yes", StringComparison.OrdinalIgnoreCase))
{
    string outputFile = Path.Combine(folders.Output, "preview.png");
    HeightMapGenerator.GeneratePreviewPng(tiffPath, outputFile);
}
