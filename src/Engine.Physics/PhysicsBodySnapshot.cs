namespace Engine.Physics;

public sealed record PhysicsBodySnapshot(
    string BodyId,
    string BodyName,
    PhysicsBodyType BodyType,
    double Mass,
    PhysicsTransform Transform,
    PhysicsBoxColliderDefinition BoxCollider,
    PhysicsAabb Aabb);
