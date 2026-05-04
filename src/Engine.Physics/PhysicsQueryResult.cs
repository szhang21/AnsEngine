namespace Engine.Physics;

public sealed record PhysicsQueryResult(IReadOnlyList<PhysicsBodySnapshot> Bodies)
{
    public int BodyCount => Bodies.Count;

    public bool HasAny => Bodies.Count > 0;
}

public sealed record PhysicsGroundQueryResult(
    string BodyId,
    PhysicsAabb Aabb,
    PhysicsGroundContactState ContactState)
{
    public bool IsAboveGround => ContactState == PhysicsGroundContactState.Above;

    public bool IsIntersectingGround => ContactState == PhysicsGroundContactState.Intersecting;

    public bool IsBelowGround => ContactState == PhysicsGroundContactState.Below;

    public static PhysicsGroundQueryResult FromAabb(string bodyId, PhysicsAabb aabb)
    {
        var state = aabb.Min.Y > 0.0f
            ? PhysicsGroundContactState.Above
            : aabb.Max.Y < 0.0f
                ? PhysicsGroundContactState.Below
                : PhysicsGroundContactState.Intersecting;
        return new PhysicsGroundQueryResult(bodyId, aabb, state);
    }
}

public enum PhysicsGroundContactState
{
    Above,
    Intersecting,
    Below
}
