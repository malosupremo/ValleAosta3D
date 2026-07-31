using BitMiracle.LibTiff.Classic;

namespace ValleAosta3D.Services;

/// <summary>
/// Provides helper methods for inspecting GeoTIFF files.
/// </summary>
public static class GeoTiffInspector
{
    /// <summary>
    /// Prints basic information about a TIFF file.
    /// </summary>
    /// <param name="path">
    /// Path of the TIFF file to inspect.
    /// </param>
    public static void PrintInfo(string path)
    {
        using Tiff? image = Tiff.Open(path, "r");

        if (image is null)
        {
            Console.WriteLine("Unable to open TIFF file.");
            return;
        }

        int width = image.GetField(TiffTag.IMAGEWIDTH)?[0].ToInt() ?? 0;
        int height = image.GetField(TiffTag.IMAGELENGTH)?[0].ToInt() ?? 0;

        int bitsPerSample = image.GetField(TiffTag.BITSPERSAMPLE)?[0].ToInt() ?? 0;
        int samplesPerPixel = image.GetField(TiffTag.SAMPLESPERPIXEL)?[0].ToInt() ?? 0;
        int compression = image.GetField(TiffTag.COMPRESSION)?[0].ToInt() ?? 0;

        Console.WriteLine();
        Console.WriteLine("GeoTIFF");
        Console.WriteLine("-------");
        Console.WriteLine($"File            : {Path.GetFileName(path)}");
        Console.WriteLine($"Width           : {width:N0}");
        Console.WriteLine($"Height          : {height:N0}");
        Console.WriteLine($"BitsPerSample   : {bitsPerSample}");
        Console.WriteLine($"SamplesPerPixel : {samplesPerPixel}");
        Console.WriteLine($"Compression     : {(Compression)compression}");

        FieldValue[]? sampleFormatField =
    image.GetField(TiffTag.SAMPLEFORMAT);

        if (sampleFormatField != null)
        {
            Console.WriteLine(
                $"SampleFormat    : {(SampleFormat)sampleFormatField[0].ToInt()}");
        }

        Console.WriteLine($"IsTiled        : {image.IsTiled()}");

        if (image.IsTiled())
        {
            Console.WriteLine($"TileWidth      : {image.GetField(TiffTag.TILEWIDTH)?[0].ToInt()}");
            Console.WriteLine($"TileHeight     : {image.GetField(TiffTag.TILELENGTH)?[0].ToInt()}");
        }

        Console.WriteLine($"TileSize : {image.TileSize():N0}");
    }

    /// <summary>
    /// Prints elevation statistics for tiled TIFF images.
    /// </summary>
    public static void PrintStatistics(string path)
    {
        using Tiff? image = Tiff.Open(path, "r");

        if (image is null)
        {
            Console.WriteLine("Unable to open TIFF file.");
            return;
        }

        float min = float.MaxValue;
        float max = float.MinValue;

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

            for (int offset = 0;
                 offset <= bytesRead - sizeof(float);
                 offset += sizeof(float))
            {
                float value =
                    BitConverter.ToSingle(
                        buffer,
                        offset);

                if (float.IsNaN(value))
                {
                    continue;
                }

                if (value <= 0)
                {
                    continue;
                }

                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }

            if ((tile + 1) % 100 == 0)
            {
                Console.Write(
                    $"\rTile {tile + 1:N0}/{tileCount:N0}");
            }
        }

        Console.WriteLine();
        Console.WriteLine();

        Console.WriteLine("Statistics");
        Console.WriteLine("----------");
        Console.WriteLine($"Min elevation : {min:N2} m");
        Console.WriteLine($"Max elevation : {max:N2} m");
    }
}