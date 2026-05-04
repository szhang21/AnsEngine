using System.Numerics;

namespace Engine.Physics;

public readonly record struct PhysicsAabb(Vector3 Min, Vector3 Max)
{
    public Vector3 Center => (Min + Max) * 0.5f;

    public Vector3 Size => Max - Min;
}
