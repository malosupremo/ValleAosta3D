using ValleAosta3D.Models;

namespace ValleAosta3D.Infrastructure;

public sealed class ApplicationFolders
{
    public string Root { get; }
    public string Cache { get; }
    public string Output { get; }

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