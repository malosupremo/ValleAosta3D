namespace ValleAosta3D.Models;

/// <summary>
/// Contains the application's configuration options.
/// </summary>
public sealed class AppOptions
{
    /// <summary>
    /// Defines the geographic area to model.
    /// </summary>
    public AreaOptions Area { get; set; } = new();

    /// <summary>
    /// Defines the physical parameters of the model.
    /// </summary>
    public ModelOptions Model { get; set; } = new();

    /// <summary>
    /// Defines the folders used by the application.
    /// </summary>
    public FolderOptions Folders { get; set; } = new();

    /// <summary>
    /// Defines settings used to download DEM data from OpenTopography.
    /// </summary>
    public OpenTopographyOptions OpenTopography { get; set; } = new();
}
