namespace Engine.Runtime;

using Engine.Contracts;
using Engine.Physics;
using Engine.Scene;

internal sealed class RuntimePhysicsOrchestrator
{
    public RuntimePhysicsUpdateResult ResolveAndWriteBack(PhysicsWorld physicsWorld, SceneGraphService sceneGraph)
    {
        ArgumentNullException.ThrowIfNull(physicsWorld);
        ArgumentNullException.ThrowIfNull(sceneGraph);

        var physicsSnapshot = physicsWorld.CreateSnapshot();
        var dynamicBodies = physicsSnapshot.Bodies
            .Where(body => body.BodyType == PhysicsBodyType.Dynamic)
            .ToArray();
        if (dynamicBodies.Length == 0)
        {
            return RuntimePhysicsUpdateResult.Success();
        }

        var sceneSnapshot = sceneGraph.CreateRuntimeSnapshot();
        foreach (var body in dynamicBodies)
        {
            var sceneObject = sceneSnapshot.Objects.FirstOrDefault(
                item => string.Equals(item.ObjectId, body.BodyId, StringComparison.Ordinal));
            if (sceneObject is null)
            {
                return RuntimePhysicsUpdateResult.FailureResult(
                    $"Physics body '{body.BodyId}' has no matching Scene object for writeback.",
                    body.BodyId);
            }

            if (!sceneObject.HasTransform || sceneObject.LocalTransform is null)
            {
                return RuntimePhysicsUpdateResult.FailureResult(
                    $"Physics body '{body.BodyId}' matching Scene object has no Transform for writeback.",
                    body.BodyId);
            }

            var desiredTransform = new PhysicsTransform(
                sceneObject.LocalTransform.Value.Position,
                sceneObject.LocalTransform.Value.Rotation,
                sceneObject.LocalTransform.Value.Scale);
            var resolveResult = physicsWorld.ApplyKinematicMove(body.BodyId, desiredTransform);
            var resolvedTransform = new SceneTransform(
                resolveResult.ResolvedTransform.Position,
                resolveResult.ResolvedTransform.Scale,
                resolveResult.ResolvedTransform.Rotation);
            var writeResult = sceneGraph.TrySetObjectTransform(body.BodyId, resolvedTransform);
            if (!writeResult.IsSuccess)
            {
                return RuntimePhysicsUpdateResult.FailureResult(
                    writeResult.Failure?.Message ?? $"Physics writeback failed for body '{body.BodyId}'.",
                    body.BodyId);
            }
        }

        return RuntimePhysicsUpdateResult.Success();
    }
}
