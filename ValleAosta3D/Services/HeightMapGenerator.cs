using System.Drawing;
using System.Drawing.Imaging;
using BitMiracle.LibTiff.Classic;

namespace ValleAosta3D.Services;

/// <summary>
/// Generates image previews from DEM GeoTIFF files.
/// </summary>
public static class HeightMapGenerator
{
    /// <summary>
    /// Creates a grayscale PNG preview from a DEM GeoTIFF.
    /// </summary>
    /// <param name="inputFile">
    /// Source GeoTIFF file.
    /// </param>
    /// <param name="outputFile">
    /// Destination PNG file.
    /// </param>
    public static void GeneratePreviewPng(
        string inputFile,
        string outputFile)
    {
        using Tiff? image = Tiff.Open(inputFile, "r");

        if (image is null)
        {
            throw new InvalidOperationException(
                "Unable to open TIFF file.");
        }

        int width =
            image.GetField(TiffTag.IMAGEWIDTH)![0].ToInt();

        int height =
            image.GetField(TiffTag.IMAGELENGTH)![0].ToInt();

        Console.WriteLine();
        Console.WriteLine("Loading DEM...");
        Console.WriteLine($"Size: {width:N0} x {height:N0}");

        float[,] elevations = new float[height, width];

        float min = float.MaxValue;
        float max = float.MinValue;

        int tileWidth =
            image.GetField(TiffTag.TILEWIDTH)![0].ToInt();

        int tileHeight =
            image.GetField(TiffTag.TILELENGTH)![0].ToInt();

        int tileSize = image.TileSize();

        byte[] buffer = new byte[tileSize];

        int tileCount = image.NumberOfTiles();

        for (int tile = 0; tile < tileCount; tile++)
        {
            int bytesRead =
                image.ReadEncodedTile(
                    tile,
                    buffer,
                    0,
                    tileSize);

            if (bytesRead <= 0)
            {
                continue;
            }

            int tilesPerRow =
                (width + tileWidth - 1) / tileWidth;

            int tileX = tile % tilesPerRow;
            int tileY = tile / tilesPerRow;

            int startX = tileX * tileWidth;
            int startY = tileY * tileHeight;

            int samplesInTile =
                bytesRead / sizeof(float);

            for (int sampleIndex = 0;
                 sampleIndex < samplesInTile;
                 sampleIndex++)
            {
                float value =
                    BitConverter.ToSingle(
                        buffer,
                        sampleIndex * sizeof(float));

                if (float.IsNaN(value))
                {
                    continue;
                }

                int localY = sampleIndex / tileWidth;
                int localX = sampleIndex % tileWidth;

                int x = startX + localX;
                int y = startY + localY;

                if (x >= width || y >= height)
                {
                    continue;
                }

                elevations[y, x] = value;

                if (value > 0)
                {
                    min = Math.Min(min, value);
                    max = Math.Max(max, value);
                }
            }

            if ((tile + 1) % 25 == 0)
            {
                Console.Write(
                    $"\rReading tile {tile + 1:N0}/{tileCount:N0}");
            }
        }

        Console.WriteLine();
        Console.WriteLine();

        Console.WriteLine("Elevation range");
        Console.WriteLine("----------------");
        Console.WriteLine($"Min : {min:N2} m");
        Console.WriteLine($"Max : {max:N2} m");

        using Bitmap bitmap =
            new(width, height, PixelFormat.Format24bppRgb);

        Console.WriteLine();
        Console.WriteLine("Generating PNG...");

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float value = elevations[y, x];

                byte gray = 0;

                if (value > 0)
                {
                    gray = (byte)(
                        (value - min) /
                        (max - min) * 255f);
                }

                bitmap.SetPixel(
                    x,
                    y,
                    Color.FromArgb(
                        gray,
                        gray,
                        gray));
            }

            if ((y + 1) % 100 == 0)
            {
                Console.Write(
                    $"\rRendering row {y + 1:N0}/{height:N0}");
            }
        }

        Directory.CreateDirectory(
            Path.GetDirectoryName(outputFile)!);

        bitmap.Save(
            outputFile,
            ImageFormat.Png);

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine($"Saved: {outputFile}");
    }

    /// <summary>
    /// Creates a grayscale PNG preview from an elevation grid.
    /// </summary>
    /// <param name="elevations">
    /// Elevation grid indexed by [y, x].
    /// </param>
    /// <param name="outputFile">
    /// Destination PNG file.
    /// </param>
    public static void GeneratePreviewPng(
        float[,] elevations,
        string outputFile)
    {
        int height = elevations.GetLength(0);
        int width = elevations.GetLength(1);

        if (width < 2 || height < 2)
        {
            throw new ArgumentException(
                "Elevation grid must be at least 2x2 samples.",
                nameof(elevations));
        }

        Console.WriteLine();
        Console.WriteLine("Elevation range");
        Console.WriteLine("----------------");

        float min = float.MaxValue;
        float max = float.MinValue;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float value = elevations[y, x];

                if (float.IsNaN(value) || value <= 0)
                {
                    continue;
                }

                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }
        }

        if (min == float.MaxValue || max == float.MinValue)
        {
            throw new InvalidOperationException(
                "Elevation grid does not contain valid positive samples.");
        }

        Console.WriteLine($"Min : {min:N2} m");
        Console.WriteLine($"Max : {max:N2} m");

        using Bitmap bitmap =
            new(width, height, PixelFormat.Format24bppRgb);

        Console.WriteLine();
        Console.WriteLine($"Generating PNG ({width:N0} x {height:N0})...");

        float range = max - min;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float value = elevations[y, x];
                byte gray = 0;

                if (!float.IsNaN(value) && value > 0)
                {
                    gray = range > 0f
                        ? (byte)Math.Clamp((value - min) / range * 255f, 0f, 255f)
                        : (byte)255;
                }

                bitmap.SetPixel(
                    x,
                    y,
                    Color.FromArgb(gray, gray, gray));
            }

            if ((y + 1) % 100 == 0)
            {
                Console.Write(
                    $"\rRendering row {y + 1:N0}/{height:N0}");
            }
        }

        Directory.CreateDirectory(
            Path.GetDirectoryName(outputFile)!);

        bitmap.Save(
            outputFile,
            ImageFormat.Png);

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine($"Saved: {outputFile}");
    }

    /// <summary>
    /// Creates a hillshade PNG preview from a DEM GeoTIFF.
    /// </summary>
    /// <param name="inputFile">
    /// Source GeoTIFF file.
    /// </param>
    /// <param name="outputFile">
    /// Destination PNG file.
    /// </param>
    public static void GenerateHillshadePng(
        string inputFile,
        string outputFile)
    {
        using Tiff? image = Tiff.Open(inputFile, "r");

        if (image is null)
        {
            throw new InvalidOperationException(
                "Unable to open TIFF file.");
        }

        int width =
            image.GetField(TiffTag.IMAGEWIDTH)![0].ToInt();

        int height =
            image.GetField(TiffTag.IMAGELENGTH)![0].ToInt();

        float[,] elevations = new float[height, width];

        int tileWidth =
            image.GetField(TiffTag.TILEWIDTH)![0].ToInt();

        int tileHeight =
            image.GetField(TiffTag.TILELENGTH)![0].ToInt();

        int tileSize = image.TileSize();

        byte[] buffer = new byte[tileSize];

        int tileCount = image.NumberOfTiles();

        Console.WriteLine();
        Console.WriteLine("Loading DEM for hillshade...");

        for (int tile = 0; tile < tileCount; tile++)
        {
            int bytesRead =
                image.ReadEncodedTile(
                    tile,
                    buffer,
                    0,
                    tileSize);

            if (bytesRead <= 0)
            {
                continue;
            }

            int tilesPerRow =
                (width + tileWidth - 1) / tileWidth;

            int tileX = tile % tilesPerRow;
            int tileY = tile / tilesPerRow;

            int startX = tileX * tileWidth;
            int startY = tileY * tileHeight;

            int samplesInTile =
                bytesRead / sizeof(float);

            for (int sampleIndex = 0;
                 sampleIndex < samplesInTile;
                 sampleIndex++)
            {
                float value =
                    BitConverter.ToSingle(
                        buffer,
                        sampleIndex * sizeof(float));

                int localY = sampleIndex / tileWidth;
                int localX = sampleIndex % tileWidth;

                int x = startX + localX;
                int y = startY + localY;

                if (x >= width || y >= height)
                {
                    continue;
                }

                elevations[y, x] = value;
            }

            if ((tile + 1) % 25 == 0)
            {
                Console.Write(
                    $"\rReading tile {tile + 1:N0}/{tileCount:N0}");
            }
        }

        Console.WriteLine();
        Console.WriteLine();

        using Bitmap bitmap =
            new(width, height, PixelFormat.Format24bppRgb);

        Console.WriteLine("Generating hillshade...");

        double azimuth = 315.0 * Math.PI / 180.0;
        double altitude = 45.0 * Math.PI / 180.0;

        double lx =
            Math.Cos(altitude) * Math.Sin(azimuth);

        double ly =
            Math.Cos(altitude) * Math.Cos(azimuth);

        double lz =
            Math.Sin(altitude);

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                float dzdx =
                    (elevations[y, x + 1] -
                     elevations[y, x - 1]) / 2f;

                float dzdy =
                    (elevations[y + 1, x] -
                     elevations[y - 1, x]) / 2f;

                double nx = -dzdx;
                double ny = -dzdy;
                double nz = 10.0;   // Scale factor for exaggeration

                double length =
                    Math.Sqrt(
                        (nx * nx) +
                        (ny * ny) +
                        (nz * nz));

                nx /= length;
                ny /= length;
                nz /= length;

                double shade =
                    (nx * lx) +
                    (ny * ly) +
                    (nz * lz);

                byte gray = (byte)Math.Clamp(
                    (shade + 1.0) * 127.5,
                    0,
                    255);

                bitmap.SetPixel(
                    x,
                    y,
                    Color.FromArgb(
                        255 - gray,
                        255 - gray,
                        255 - gray));
            }

            if ((y + 1) % 100 == 0)
            {
                Console.Write(
                    $"\rRendering row {y + 1:N0}/{height:N0}");
            }
        }

        Directory.CreateDirectory(
            Path.GetDirectoryName(outputFile)!);

        bitmap.Save(
            outputFile,
            ImageFormat.Png);

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine($"Saved: {outputFile}");
    }
}