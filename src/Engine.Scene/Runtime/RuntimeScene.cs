namespace Engine.Scene;

using Engine.Contracts;
using Engine.SceneData;

internal sealed class RuntimeScene
{
    private readonly List<SceneRuntimeObject> mObjects = new();

    public int ObjectCount => mObjects.Count;

    public int UpdateFrameCount { get; private set; }

    public double AccumulatedUpdateSeconds { get; private set; }

    public IReadOnlyList<SceneRuntimeObject> Objects => mObjects;

    public SceneCameraRuntimeState Camera { get; private set; } = SceneCameraRuntimeState.CreateDefault();

    public SceneRuntimeObject CreateObject(
        int nodeId,
        string objectId,
        string objectName,
        SceneTransformComponent? transform = null,
        SceneMeshRendererComponent? meshRenderer = null)
    {
        var runtimeObject = new SceneRuntimeObject(nodeId, objectId, objectName, transform, meshRenderer);
        mObjects.Add(runtimeObject);
        return runtimeObject;
    }

    public void Clear()
    {
        mObjects.Clear();
        Camera = SceneCameraRuntimeState.CreateDefault();
        ResetUpdateStatistics();
    }

    public void LoadFromDescription(SceneDescription sceneDescription)
    {
        ArgumentNullException.ThrowIfNull(sceneDescription);

        Clear();
        Camera = SceneCameraRuntimeState.FromDescription(sceneDescription.Camera);

        for (var index = 0; index < sceneDescription.Objects.Count; index += 1)
        {
            var objectDescription = sceneDescription.Objects[index];
            CreateObject(
                index + 1,
                objectDescription.ObjectId,
                objectDescription.ObjectName,
                objectDescription.TransformComponent is null
                    ? null
                    : SceneTransformComponent.FromDescription(objectDescription.TransformComponent),
                objectDescription.MeshRendererComponent is null
                    ? null
                    : SceneMeshRendererComponent.FromDescription(objectDescription.MeshRendererComponent));
        }
    }

    public void Update(SceneUpdateContext context)
    {
        UpdateFrameCount += 1;
        AccumulatedUpdateSeconds += context.DeltaSeconds;
    }

    public IReadOnlyList<SceneRenderItem> BuildRenderItems()
    {
        var renderItems = new List<SceneRenderItem>(mObjects.Count);
        foreach (var runtimeObject in mObjects)
        {
            if (runtimeObject.Transform is null || runtimeObject.MeshRenderer is null)
            {
                continue;
            }

            renderItems.Add(
                new SceneRenderItem(
                    runtimeObject.NodeId,
                    runtimeObject.MeshRenderer.Mesh,
                    runtimeObject.MeshRenderer.Material,
                    runtimeObject.Transform.ToSceneTransform()));
        }

        return renderItems;
    }

    public SceneCamera BuildCamera(float aspectRatio, float nearPlane, float farPlane)
    {
        return Camera.BuildCamera(aspectRatio, nearPlane, farPlane);
    }

    public RuntimeSceneSnapshot CreateSnapshot()
    {
        var objectSnapshots = mObjects
            .Select(item => item.CreateSnapshot())
            .ToArray();
        return new RuntimeSceneSnapshot(
            objectSnapshots,
            Camera.CreateSnapshot(),
            UpdateFrameCount,
            AccumulatedUpdateSeconds);
    }

    public SceneRuntimeObjectSnapshot? FindObject(string objectId)
    {
        for (var index = 0; index < mObjects.Count; index += 1)
        {
            if (string.Equals(mObjects[index].ObjectId, objectId, StringComparison.Ordinal))
            {
                return mObjects[index].CreateSnapshot();
            }
        }

        return null;
    }

    public SceneTransformWriteResult TrySetObjectTransform(string objectId, SceneTransform transform)
    {
        if (string.IsNullOrWhiteSpace(objectId))
        {
            return SceneTransformWriteResult.FailureResult(
                new SceneTransformWriteFailure(
                    SceneTransformWriteFailureKind.ObjectNotFound,
                    "Scene object id must not be null or whitespace.",
                    objectId));
        }

        if (!IsValidTransform(transform))
        {
            return SceneTransformWriteResult.FailureResult(
                new SceneTransformWriteFailure(
                    SceneTransformWriteFailureKind.InvalidTransform,
                    $"Scene object id '{objectId}' transform contains non-finite values.",
                    objectId));
        }

        for (var index = 0; index < mObjects.Count; index += 1)
        {
            if (!string.Equals(mObjects[index].ObjectId, objectId, StringComparison.Ordinal))
            {
                continue;
            }

            var transformComponent = mObjects[index].Transform;
            if (transformComponent is null)
            {
                return SceneTransformWriteResult.FailureResult(
                    new SceneTransformWriteFailure(
                        SceneTransformWriteFailureKind.MissingTransform,
                        $"Scene object id '{objectId}' has no Transform component.",
                        objectId));
            }

            transformComponent.SetLocalTransform(
                transform.Position,
                transform.Rotation,
                transform.Scale);
            return SceneTransformWriteResult.Success();
        }

        return SceneTransformWriteResult.FailureResult(
            new SceneTransformWriteFailure(
                SceneTransformWriteFailureKind.ObjectNotFound,
                $"Scene object id '{objectId}' was not found.",
                objectId));
    }

    public SceneScriptObjectBindResult BindScriptObject(string objectId)
    {
        for (var index = 0; index < mObjects.Count; index += 1)
        {
            if (!string.Equals(mObjects[index].ObjectId, objectId, StringComparison.Ordinal))
            {
                continue;
            }

            if (mObjects[index].Transform is null)
            {
                return SceneScriptObjectBindResult.FailureResult(
                    new SceneScriptObjectBindFailure(
                        SceneScriptObjectBindFailureKind.MissingTransform,
                        $"Scene object id '{objectId}' has no Transform component.",
                        objectId));
            }

            return SceneScriptObjectBindResult.Success(new SceneScriptObjectHandle(mObjects[index]));
        }

        return SceneScriptObjectBindResult.FailureResult(
            new SceneScriptObjectBindFailure(
                SceneScriptObjectBindFailureKind.ObjectNotFound,
                $"Scene object id '{objectId}' was not found.",
                objectId));
    }

    private void ResetUpdateStatistics()
    {
        UpdateFrameCount = 0;
        AccumulatedUpdateSeconds = 0.0d;
    }

    private static bool IsValidTransform(SceneTransform transform)
    {
        return IsFinite(transform.Position) &&
               IsFinite(transform.Scale) &&
               IsFinite(transform.Rotation);
    }

    private static bool IsFinite(System.Numerics.Vector3 value)
    {
        return float.IsFinite(value.X) &&
               float.IsFinite(value.Y) &&
               float.IsFinite(value.Z);
    }

    private static bool IsFinite(System.Numerics.Quaternion value)
    {
        return float.IsFinite(value.X) &&
               float.IsFinite(value.Y) &&
               float.IsFinite(value.Z) &&
               float.IsFinite(value.W);
    }
}
