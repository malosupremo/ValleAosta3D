using ValleAosta3D.Models;

namespace ValleAosta3D.Infrastructure;

/// <summary>
/// Provides the folder paths used by the application.
/// </summary>
public sealed class ApplicationFolders
{
    /// <summary>
    /// Root path of the project.
    /// </summary>
    public string Root { get; }

    /// <summary>
    /// Path of the cache folder.
    /// </summary>
    public string Cache { get; }

    /// <summary>
    /// Path of the output folder.
    /// </summary>
    public string Output { get; }

    /// <summary>
    /// Initializes the folder paths and creates them if they do not exist.
    /// </summary>
    /// <param name="options">Application configuration options.</param>
    public ApplicationFolders(AppOptions options)
    {
        Root = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                ".."));

        Cache = Path.Combine(Root, options.Folders.Cache);
        Output = Path.Combine(Root, options.Folders.Output);

        Directory.CreateDirectory(Cache);
        Directory.CreateDirectory(Output);
    }
}
