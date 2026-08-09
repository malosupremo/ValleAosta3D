namespace ValleAosta3D.Services;

/// <summary>
/// Resamples elevation grids to target dimensions.
/// </summary>
public static class HeightMapResampler
{
    /// <summary>
    /// Resamples a source elevation grid to a target size using bilinear interpolation.
    /// </summary>
    /// <param name="source">Source elevation grid indexed by [y, x].</param>
    /// <param name="targetWidth">Target width in samples.</param>
    /// <param name="targetHeight">Target height in samples.</param>
    /// <returns>A new elevation grid with the requested dimensions.</returns>
    public static float[,] ResizeBilinear(
        float[,] source,
        int targetWidth,
        int targetHeight)
    {
        if (targetWidth <= 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetWidth),
                "Target width must be greater than 1.");
        }

        if (targetHeight <= 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetHeight),
                "Target height must be greater than 1.");
        }

        int sourceHeight = source.GetLength(0);
        int sourceWidth = source.GetLength(1);

        if (sourceWidth <= 1 || sourceHeight <= 1)
        {
            throw new ArgumentException(
                "Source grid must have at least 2x2 samples.",
                nameof(source));
        }

        float[,] target = new float[targetHeight, targetWidth];

        double xScale = (sourceWidth - 1d) / (targetWidth - 1d);
        double yScale = (sourceHeight - 1d) / (targetHeight - 1d);

        for (int y = 0; y < targetHeight; y++)
        {
            double sourceY = y * yScale;
            int y0 = (int)Math.Floor(sourceY);
            int y1 = Math.Min(y0 + 1, sourceHeight - 1);
            double yLerp = sourceY - y0;

            for (int x = 0; x < targetWidth; x++)
            {
                double sourceX = x * xScale;
                int x0 = (int)Math.Floor(sourceX);
                int x1 = Math.Min(x0 + 1, sourceWidth - 1);
                double xLerp = sourceX - x0;

                float topLeft = source[y0, x0];
                float topRight = source[y0, x1];
                float bottomLeft = source[y1, x0];
                float bottomRight = source[y1, x1];

                double top = Lerp(topLeft, topRight, xLerp);
                double bottom = Lerp(bottomLeft, bottomRight, xLerp);
                target[y, x] = (float)Lerp(top, bottom, yLerp);
            }

            if ((y + 1) % 100 == 0)
            {
                Console.Write($"\rResampling row {y + 1:N0}/{targetHeight:N0}");
            }
        }

        Console.WriteLine();

        return target;
    }

    private static double Lerp(
        double left,
        double right,
        double factor)
    {
        return left + ((right - left) * factor);
    }
}
