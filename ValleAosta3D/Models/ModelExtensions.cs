namespace ValleAosta3D.Models;

public static class ModelExtensions
{
    public static int GetWidthMm(this ModelOptions model)
    {
        return model.TilesX * model.TileSizeMm;
    }

    public static int GetHeightMm(this ModelOptions model)
    {
        return model.TilesY * model.TileSizeMm;
    }
}
