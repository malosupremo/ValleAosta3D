namespace ValleAosta3D.Models;

public sealed class AppOptions
{
    public AreaOptions Area { get; set; } = new();
    public ModelOptions Model { get; set; } = new();
    public FolderOptions Folders { get; set; } = new();
}
