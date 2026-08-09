namespace ValleAosta3D.Models;

/// <summary>
/// Defines OpenTopography API settings used for DEM download.
/// </summary>
public sealed class OpenTopographyOptions
{
    /// <summary>
    /// Base URL of the OpenTopography portal.
    /// </summary>
    public string BaseUrl { get; set; } = "https://portal.opentopography.org";

    /// <summary>
    /// API key used to authenticate DEM requests.
    /// This value should be stored in user secrets.
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// DEM dataset type, for example <c>COP30</c>.
    /// </summary>
    public string DemType { get; set; } = "COP30";

    /// <summary>
    /// Output format requested to OpenTopography.
    /// </summary>
    public string OutputFormat { get; set; } = "GTiff";
}
