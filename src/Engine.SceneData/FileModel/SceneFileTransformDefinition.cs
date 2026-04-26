using System.Numerics;

namespace Engine.SceneData;

public sealed record SceneFileTransformDefinition(
    Vector3? Position,
    Quaternion? Rotation,
    Vector3? Scale);
