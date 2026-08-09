using ValleAosta3D.Models;

namespace ValleAosta3D.Infrastructure;

/// <summary>
/// Provides the folder paths used by the application.
/// </summary>
/// <param name="options">Application configuration options.</param>
public sealed class ApplicationFolders(AppOptions options)
{
    /// <summary>
    /// Root path of the project.
    /// </summary>
    public string Root { get; } = GetProjectRoot();

    /// <summary>
    /// Path of the cache root folder.
    /// </summary>
    public string CacheRoot { get; } = EnsureDirectory(
        Path.Combine(GetProjectRoot(), options.Folders.Cache));

    /// <summary>
    /// Path of the raw DEM cache folder.
    /// </summary>
    public string RawCache { get; } = EnsureDirectory(
        Path.Combine(
            EnsureDirectory(Path.Combine(GetProjectRoot(), options.Folders.Cache)),
            "Raw"));

    /// <summary>
    /// Path of the resampled DEM cache folder.
    /// </summary>
    public string ResampledCache { get; } = EnsureDirectory(
        Path.Combine(
            EnsureDirectory(Path.Combine(GetProjectRoot(), options.Folders.Cache)),
            "Resampled"));

    /// <summary>
    /// Path of the raw DEM cache folder.
    /// </summary>
    public string Cache => RawCache;

    /// <summary>
    /// Path of the output folder.
    /// </summary>
    public string Output { get; } = EnsureDirectory(
        Path.Combine(GetProjectRoot(), options.Folders.Output));

    private static string GetProjectRoot()
    {
        return Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                ".."));
    }

    private static string EnsureDirectory(string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }
}
