using BitMiracle.LibTiff.Classic;

namespace ValleAosta3D.Services;

/// <summary>
/// Reads elevation samples from a GeoTIFF DEM file.
/// </summary>
public sealed class GeoTiffReader
{
    /// <summary>
    /// Reads elevation samples from a GeoTIFF file as a two-dimensional array.
    /// </summary>
    /// <param name="path">Absolute or relative path of the source GeoTIFF file.</param>
    /// <returns>Elevation matrix in row-major order indexed by [y, x].</returns>
    public float[,] ReadElevations(string path)
    {
        using Tiff? image = Tiff.Open(path, "r");

        if (image is null)
        {
            throw new InvalidOperationException("Unable to open TIFF file.");
        }

        int width = image.GetField(TiffTag.IMAGEWIDTH)![0].ToInt();
        int height = image.GetField(TiffTag.IMAGELENGTH)![0].ToInt();

        float[,] elevations = new float[height, width];

        int tileWidth = image.GetField(TiffTag.TILEWIDTH)![0].ToInt();
        int tileHeight = image.GetField(TiffTag.TILELENGTH)![0].ToInt();
        int tileSize = image.TileSize();
        int tileCount = image.NumberOfTiles();
        int tilesPerRow = (width + tileWidth - 1) / tileWidth;

        byte[] buffer = new byte[tileSize];

        for (int tile = 0; tile < tileCount; tile++)
        {
            int bytesRead = image.ReadEncodedTile(
                tile,
                buffer,
                0,
                tileSize);

            if (bytesRead <= 0)
            {
                continue;
            }

            int tileX = tile % tilesPerRow;
            int tileY = tile / tilesPerRow;
            int startX = tileX * tileWidth;
            int startY = tileY * tileHeight;

            int samplesInTile = bytesRead / sizeof(float);

            for (int sampleIndex = 0; sampleIndex < samplesInTile; sampleIndex++)
            {
                float value = BitConverter.ToSingle(
                    buffer,
                    sampleIndex * sizeof(float));

                int localY = sampleIndex / tileWidth;
                int localX = sampleIndex % tileWidth;
                int x = startX + localX;
                int y = startY + localY;

                if (x >= width || y >= height || float.IsNaN(value))
                {
                    continue;
                }

                elevations[y, x] = value;
            }

            if ((tile + 1) % 25 == 0)
            {
                Console.Write($"\rReading DEM tile {tile + 1:N0}/{tileCount:N0}");
            }
        }

        Console.WriteLine();

        return elevations;
    }
}
