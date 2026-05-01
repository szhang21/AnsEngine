using System.Numerics;

namespace Engine.Scene;

public sealed record SceneCameraRuntimeSnapshot(
    Vector3 Position,
    Vector3 Target,
    float FieldOfViewRadians);
