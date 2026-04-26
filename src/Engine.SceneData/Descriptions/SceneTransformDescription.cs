using System.Numerics;

namespace Engine.SceneData;

public sealed record SceneTransformDescription(
    Vector3 Position,
    Quaternion Rotation,
    Vector3 Scale)
{
    public static SceneTransformDescription Identity { get; } = new(
        Vector3.Zero,
        Quaternion.Identity,
        Vector3.One);
}
