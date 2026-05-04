using System.Numerics;

namespace Engine.Physics;

public readonly record struct PhysicsTransform(Vector3 Position, Quaternion Rotation, Vector3 Scale)
{
    public static PhysicsTransform Identity { get; } = new(Vector3.Zero, Quaternion.Identity, Vector3.One);
}
