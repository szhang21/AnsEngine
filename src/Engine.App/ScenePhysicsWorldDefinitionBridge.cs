using Engine.Physics;
using Engine.SceneData;

namespace Engine.App;

internal static class ScenePhysicsWorldDefinitionBridge
{
    public static PhysicsWorldDefinition CreateDefinition(SceneDescription sceneDescription)
    {
        ArgumentNullException.ThrowIfNull(sceneDescription);

        var bodies = new List<PhysicsBodyDefinition>();
        foreach (var sceneObject in sceneDescription.Objects)
        {
            var transformComponent = sceneObject.TransformComponent;
            var rigidBodyComponent = sceneObject.RigidBodyComponent;
            var boxColliderComponent = sceneObject.BoxColliderComponent;
            if (transformComponent is null || rigidBodyComponent is null || boxColliderComponent is null)
            {
                continue;
            }

            bodies.Add(
                new PhysicsBodyDefinition(
                    sceneObject.ObjectId,
                    sceneObject.ObjectName,
                    ConvertBodyType(rigidBodyComponent.BodyType),
                    rigidBodyComponent.Mass,
                    new PhysicsTransform(
                        transformComponent.Transform.Position,
                        transformComponent.Transform.Rotation,
                        transformComponent.Transform.Scale),
                    new PhysicsBoxColliderDefinition(boxColliderComponent.Size, boxColliderComponent.Center)));
        }

        return new PhysicsWorldDefinition(bodies);
    }

    public static PhysicsWorld CreateWorld(SceneDescription sceneDescription)
    {
        try
        {
            return PhysicsWorld.Load(CreateDefinition(sceneDescription));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            throw new InvalidOperationException("App Physics bridge failed to initialize PhysicsWorld from SceneDescription.", ex);
        }
    }

    private static PhysicsBodyType ConvertBodyType(SceneRigidBodyType bodyType)
    {
        return bodyType switch
        {
            SceneRigidBodyType.Static => PhysicsBodyType.Static,
            SceneRigidBodyType.Dynamic => PhysicsBodyType.Dynamic,
            _ => throw new ArgumentOutOfRangeException(nameof(bodyType), bodyType, "Unsupported SceneData rigid body type.")
        };
    }
}
