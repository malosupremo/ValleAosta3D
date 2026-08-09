namespace ValleAosta3D.Services;

/// <summary>
/// Reads rectangular windows from cached resampled DEM binary files.
/// </summary>
public sealed class ResampledDemWindowReader
{
    /// <summary>
    /// Reads a rectangular elevation window from a row-major float32 DEM file.
    /// </summary>
    /// <param name="path">Path of the source .f32 file.</param>
    /// <param name="sourceWidth">Full source width in samples.</param>
    /// <param name="sourceHeight">Full source height in samples.</param>
    /// <param name="startX">Window start X in source samples.</param>
    /// <param name="startY">Window start Y in source samples.</param>
    /// <param name="width">Window width in samples.</param>
    /// <param name="height">Window height in samples.</param>
    /// <returns>Window values as [y, x] matrix.</returns>
    public float[,] ReadWindow(
        string path,
        int sourceWidth,
        int sourceHeight,
        int startX,
        int startY,
        int width,
        int height)
    {
        ValidateArguments(
            sourceWidth,
            sourceHeight,
            startX,
            startY,
            width,
            height);

        float[,] result = new float[height, width];

        using FileStream stream = new(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        using BinaryReader reader = new(stream);
        int bytesPerSample = sizeof(float);

        for (int y = 0; y < height; y++)
        {
            long sourceRowIndex = startY + y;
            long sourceOffsetSamples = (sourceRowIndex * sourceWidth) + startX;
            long sourceOffsetBytes = sourceOffsetSamples * bytesPerSample;

            stream.Seek(sourceOffsetBytes, SeekOrigin.Begin);

            for (int x = 0; x < width; x++)
            {
                result[y, x] = reader.ReadSingle();
            }
        }

        return result;
    }

    private static void ValidateArguments(
        int sourceWidth,
        int sourceHeight,
        int startX,
        int startY,
        int width,
        int height)
    {
        if (sourceWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceWidth),
                "Source width must be greater than zero.");
        }

        if (sourceHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceHeight),
                "Source height must be greater than zero.");
        }

        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                "Window width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(height),
                "Window height must be greater than zero.");
        }

        if (startX < 0 || startY < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startX),
                "Window start must be non-negative.");
        }

        if (startX + width > sourceWidth || startY + height > sourceHeight)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                "Requested window exceeds source bounds.");
        }
    }
}
