namespace Engine.Physics;

public sealed record PhysicsWorldDefinition(IReadOnlyList<PhysicsBodyDefinition> Bodies);
