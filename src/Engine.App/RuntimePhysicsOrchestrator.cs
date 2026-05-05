using Engine.Contracts;
using Engine.Physics;
using Engine.Scene;

namespace Engine.App;

internal sealed class RuntimePhysicsOrchestrator
{
    public RuntimePhysicsUpdateResult ResolveAndWriteBack(PhysicsWorld physicsWorld, ISceneRuntime sceneRuntime)
    {
        ArgumentNullException.ThrowIfNull(physicsWorld);
        ArgumentNullException.ThrowIfNull(sceneRuntime);

        var physicsSnapshot = physicsWorld.CreateSnapshot();
        var dynamicBodies = physicsSnapshot.Bodies
            .Where(body => body.BodyType == PhysicsBodyType.Dynamic)
            .ToArray();
        if (dynamicBodies.Length == 0)
        {
            return RuntimePhysicsUpdateResult.Success();
        }

        var sceneSnapshot = sceneRuntime.CreateRuntimeSnapshot();
        foreach (var body in dynamicBodies)
        {
            var sceneObject = sceneSnapshot.Objects.FirstOrDefault(
                item => string.Equals(item.ObjectId, body.BodyId, StringComparison.Ordinal));
            if (sceneObject is null)
            {
                return RuntimePhysicsUpdateResult.FailureResult(
                    $"Physics body '{body.BodyId}' has no matching Scene object for writeback.");
            }

            if (!sceneObject.HasTransform || sceneObject.LocalTransform is null)
            {
                return RuntimePhysicsUpdateResult.FailureResult(
                    $"Physics body '{body.BodyId}' matching Scene object has no Transform for writeback.");
            }

            var desiredTransform = new PhysicsTransform(
                sceneObject.LocalTransform.Value.Position,
                sceneObject.LocalTransform.Value.Rotation,
                sceneObject.LocalTransform.Value.Scale);
            var resolveResult = physicsWorld.ResolveKinematicMove(body.BodyId, desiredTransform);
            var resolvedTransform = new SceneTransform(
                resolveResult.ResolvedTransform.Position,
                resolveResult.ResolvedTransform.Scale,
                resolveResult.ResolvedTransform.Rotation);
            var writeResult = sceneRuntime.TrySetObjectTransform(body.BodyId, resolvedTransform);
            if (!writeResult.IsSuccess)
            {
                return RuntimePhysicsUpdateResult.FailureResult(
                    writeResult.Failure?.Message ?? $"Physics writeback failed for body '{body.BodyId}'.");
            }
        }

        return RuntimePhysicsUpdateResult.Success();
    }
}

internal sealed record RuntimePhysicsUpdateResult
{
    private RuntimePhysicsUpdateResult(bool isSuccess, string? failureMessage)
    {
        IsSuccess = isSuccess;
        FailureMessage = failureMessage;
    }

    public bool IsSuccess { get; }

    public string? FailureMessage { get; }

    public static RuntimePhysicsUpdateResult Success()
    {
        return new RuntimePhysicsUpdateResult(true, null);
    }

    public static RuntimePhysicsUpdateResult FailureResult(string failureMessage)
    {
        return new RuntimePhysicsUpdateResult(false, failureMessage);
    }
}
