namespace ValleAosta3D.Models;

/// <summary>
/// Represents the folders used by the application.
/// </summary>
public sealed class FolderOptions
{
    /// <summary>
    /// Name of the root folder used as cache.
    /// </summary>
    public string Cache { get; set; } = "";

    /// <summary>
    /// Name of the folder used for output.
    /// </summary>
    public string Output { get; set; } = "";
}
