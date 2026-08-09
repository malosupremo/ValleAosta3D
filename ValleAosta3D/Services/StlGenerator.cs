using System.Numerics;
using ValleAosta3D.Models;

namespace ValleAosta3D.Services;

/// <summary>
/// Generates binary STL meshes from elevation windows.
/// </summary>
public sealed class StlGenerator
{
    /// <summary>
    /// Generates a watertight binary STL from an elevation grid.
    /// </summary>
    /// <param name="elevations">Elevation matrix [y, x] in meters.</param>
    /// <param name="outputPath">Destination STL file path.</param>
    /// <param name="options">Generation options for scale and base thickness.</param>
    /// <returns>Total number of triangles written to the STL file.</returns>
    public static int GenerateBinaryStl(
        float[,] elevations,
        string outputPath,
        StlGenerationOptions options)
    {
        ValidateArguments(elevations, outputPath, options);

        int height = elevations.GetLength(0);
        int width = elevations.GetLength(1);

        int topTriangles = (width - 1) * (height - 1) * 2;
        int sideTriangles = (4 * (width - 1)) + (4 * (height - 1));
        int bottomTriangles = 2;
        int triangleCount = topTriangles + sideTriangles + bottomTriangles;

        string? directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using FileStream stream = new(
            outputPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);
        using BinaryWriter writer = new(stream);

        WriteHeader(writer, triangleCount);

        WriteTopSurface(writer, elevations, options);
        WriteSideWalls(writer, elevations, options);
        WriteBottom(writer, width, height, options);

        return triangleCount;
    }

    private static void WriteHeader(
        BinaryWriter writer,
        int triangleCount)
    {
        byte[] header = new byte[80];
        byte[] text = System.Text.Encoding.ASCII.GetBytes("ValleAosta3D terrain");
        Array.Copy(text, header, Math.Min(text.Length, header.Length));
        writer.Write(header);
        writer.Write((uint)triangleCount);
    }

    private static void WriteTopSurface(
        BinaryWriter writer,
        float[,] elevations,
        StlGenerationOptions options)
    {
        int height = elevations.GetLength(0);
        int width = elevations.GetLength(1);

        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                Vector3 p00 = GetTopVertex(elevations, x, y, height, options);
                Vector3 p10 = GetTopVertex(elevations, x + 1, y, height, options);
                Vector3 p01 = GetTopVertex(elevations, x, y + 1, height, options);
                Vector3 p11 = GetTopVertex(elevations, x + 1, y + 1, height, options);

                WriteTriangle(writer, p00, p10, p11);
                WriteTriangle(writer, p00, p11, p01);
            }
        }
    }

    private static void WriteSideWalls(
        BinaryWriter writer,
        float[,] elevations,
        StlGenerationOptions options)
    {
        int height = elevations.GetLength(0);
        int width = elevations.GetLength(1);

        // North edge (y = 0)
        for (int x = 0; x < width - 1; x++)
        {
            Vector3 a = GetTopVertex(elevations, x, 0, height, options);
            Vector3 b = GetTopVertex(elevations, x + 1, 0, height, options);
            Vector3 c = GetBottomVertex(x + 1, 0, height, options);
            Vector3 d = GetBottomVertex(x, 0, height, options);
            WriteTriangle(writer, a, c, b);
            WriteTriangle(writer, a, d, c);
        }

        // South edge (y = height - 1)
        for (int x = 0; x < width - 1; x++)
        {
            int y = height - 1;
            Vector3 a = GetTopVertex(elevations, x, y, height, options);
            Vector3 b = GetTopVertex(elevations, x + 1, y, height, options);
            Vector3 c = GetBottomVertex(x + 1, y, height, options);
            Vector3 d = GetBottomVertex(x, y, height, options);
            WriteTriangle(writer, a, b, c);
            WriteTriangle(writer, a, c, d);
        }

        // West edge (x = 0)
        for (int y = 0; y < height - 1; y++)
        {
            Vector3 a = GetTopVertex(elevations, 0, y, height, options);
            Vector3 b = GetTopVertex(elevations, 0, y + 1, height, options);
            Vector3 c = GetBottomVertex(0, y + 1, height, options);
            Vector3 d = GetBottomVertex(0, y, height, options);
            WriteTriangle(writer, a, b, c);
            WriteTriangle(writer, a, c, d);
        }

        // East edge (x = width - 1)
        for (int y = 0; y < height - 1; y++)
        {
            int x = width - 1;
            Vector3 a = GetTopVertex(elevations, x, y, height, options);
            Vector3 b = GetTopVertex(elevations, x, y + 1, height, options);
            Vector3 c = GetBottomVertex(x, y + 1, height, options);
            Vector3 d = GetBottomVertex(x, y, height, options);
            WriteTriangle(writer, a, c, b);
            WriteTriangle(writer, a, d, c);
        }
    }

    private static void WriteBottom(
        BinaryWriter writer,
        int width,
        int height,
        StlGenerationOptions options)
    {
        Vector3 p00 = GetBottomVertex(0, 0, height, options);
        Vector3 p10 = GetBottomVertex(width - 1, 0, height, options);
        Vector3 p01 = GetBottomVertex(0, height - 1, height, options);
        Vector3 p11 = GetBottomVertex(width - 1, height - 1, height, options);

        WriteTriangle(writer, p00, p11, p10);
        WriteTriangle(writer, p00, p01, p11);
    }

    private static Vector3 GetTopVertex(
        float[,] elevations,
        int x,
        int y,
        int height,
        StlGenerationOptions options)
    {
        float elevationMeters = elevations[y, x];
        double z = options.BaseThicknessMm + (elevationMeters * options.ElevationScaleMmPerMeter);

        // STL Y axis is flipped so north/south orientation matches the PNG preview.
        int flippedY = height - 1 - y;
        return new Vector3(
            (float)(x * options.HorizontalStepMm),
            (float)(flippedY * options.HorizontalStepMm),
            (float)z);
    }

    private static Vector3 GetBottomVertex(
        int x,
        int y,
        int height,
        StlGenerationOptions options)
    {
        int flippedY = height - 1 - y;
        return new Vector3(
            (float)(x * options.HorizontalStepMm),
            (float)(flippedY * options.HorizontalStepMm),
            0f);
    }

    private static void WriteTriangle(
        BinaryWriter writer,
        Vector3 p1,
        Vector3 p2,
        Vector3 p3)
    {
        // Flip winding so outward normals are produced with the flipped STL Y axis.
        Vector3 v2 = p3;
        Vector3 v3 = p2;

        Vector3 normal = Vector3.Normalize(Vector3.Cross(v2 - p1, v3 - p1));

        if (float.IsNaN(normal.X) || float.IsNaN(normal.Y) || float.IsNaN(normal.Z))
        {
            normal = Vector3.Zero;
        }

        writer.Write(normal.X);
        writer.Write(normal.Y);
        writer.Write(normal.Z);

        writer.Write(p1.X);
        writer.Write(p1.Y);
        writer.Write(p1.Z);

        writer.Write(v2.X);
        writer.Write(v2.Y);
        writer.Write(v2.Z);

        writer.Write(v3.X);
        writer.Write(v3.Y);
        writer.Write(v3.Z);

        writer.Write((ushort)0);
    }

    private static void ValidateArguments(
        float[,] elevations,
        string outputPath,
        StlGenerationOptions options)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException(
                "Output path cannot be null or empty.",
                nameof(outputPath));
        }

        if (elevations.GetLength(0) < 2 || elevations.GetLength(1) < 2)
        {
            throw new ArgumentException(
                "Elevation grid must be at least 2x2.",
                nameof(elevations));
        }

        if (options.HorizontalStepMm <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "HorizontalStepMm must be greater than zero.");
        }

        if (options.ElevationScaleMmPerMeter <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "ElevationScaleMmPerMeter must be greater than zero.");
        }

        if (options.BaseThicknessMm < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "BaseThicknessMm cannot be negative.");
        }
    }
}
