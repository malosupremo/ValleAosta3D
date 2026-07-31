using ValleAosta3D.Infrastructure;

/// <summary>
/// Provides cache paths for downloaded DEM files.
/// </summary>
public sealed class DemCacheService
{
    private readonly ApplicationFolders _folders;

    public DemCacheService(ApplicationFolders folders)
    {
        _folders = folders;
    }

    public string GetCachedDemPath(string datasetName)
    {
        return Path.Combine(
            _folders.Cache,
            $"{datasetName}.tif");
    }

    public bool Exists(string datasetName)
    {
        return File.Exists(
            GetCachedDemPath(datasetName));
    }
}