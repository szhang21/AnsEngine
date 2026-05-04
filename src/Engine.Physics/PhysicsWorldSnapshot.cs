namespace Engine.Physics;

public sealed record PhysicsWorldSnapshot(
    IReadOnlyList<PhysicsBodySnapshot> Bodies,
    long StepCount,
    double AccumulatedFixedSeconds)
{
    public int BodyCount => Bodies.Count;
}
