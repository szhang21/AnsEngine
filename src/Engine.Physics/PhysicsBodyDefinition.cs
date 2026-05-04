namespace Engine.Physics;

public sealed record PhysicsBodyDefinition(
    string BodyId,
    string BodyName,
    PhysicsBodyType BodyType,
    double Mass,
    PhysicsTransform? Transform,
    PhysicsBoxColliderDefinition? BoxCollider);
