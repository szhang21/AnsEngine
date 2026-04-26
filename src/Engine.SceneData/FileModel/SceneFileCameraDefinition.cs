using System.Numerics;

namespace Engine.SceneData;

public sealed record SceneFileCameraDefinition(
    Vector3 Position,
    Vector3 Target,
    float? FieldOfViewRadians);
