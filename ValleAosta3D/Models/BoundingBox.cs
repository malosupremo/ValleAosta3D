namespace ValleAosta3D.Models;

/// <summary>
/// Represents the geographic rectangle covered by the model.
/// </summary>
/// <param name="South">Minimum latitude of the southern edge.</param>
/// <param name="West">Minimum longitude of the western edge.</param>
/// <param name="North">Maximum latitude of the northern edge.</param>
/// <param name="East">Maximum longitude of the eastern edge.</param>
public sealed record BoundingBox(
    double South,
    double West,
    double North,
    double East
    );
