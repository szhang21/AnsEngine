using System.Numerics;

namespace Engine.SceneData;

public sealed record SceneCameraDescription(
    Vector3 Position,
    Vector3 Target,
    float FieldOfViewRadians);
