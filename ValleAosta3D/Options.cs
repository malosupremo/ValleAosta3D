namespace ValleAosta3D;

public sealed class AreaOptions
{
    public double South { get; set; }
    public double West { get; set; }
    public double North { get; set; }
    public double East { get; set; }
}

public sealed class ModelOptions
{
    public int Scale { get; set; }
    public int VerticalExaggeration { get; set; }
    public int TilesX { get; set; }
    public int TilesY { get; set; }
    public int TileSizeMm { get; set; }
}

public sealed class FolderOptions
{
    public string Cache { get; set; } = "";
    public string Output { get; set; } = "";
}