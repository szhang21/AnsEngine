namespace Engine.Scene;

using Engine.Contracts;
using Engine.SceneData;
using System.Numerics;

internal sealed class RuntimeScene
{
    private const float kDefaultRotationRadiansPerSecond = MathF.PI * 0.5f;

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
                SceneTransformComponent.FromDescription(objectDescription.LocalTransform),
                SceneMeshRendererComponent.FromDescription(objectDescription));
        }
    }

    public void Update(SceneUpdateContext context)
    {
        UpdateFrameCount += 1;
        AccumulatedUpdateSeconds += context.DeltaSeconds;
        ApplyDefaultRotationSmokeBehavior(context);
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

    private void ResetUpdateStatistics()
    {
        UpdateFrameCount = 0;
        AccumulatedUpdateSeconds = 0.0d;
    }

    private void ApplyDefaultRotationSmokeBehavior(SceneUpdateContext context)
    {
        if (context.DeltaSeconds == 0.0d)
        {
            return;
        }

        foreach (var runtimeObject in mObjects)
        {
            if (runtimeObject.Transform is null || runtimeObject.MeshRenderer is null)
            {
                continue;
            }

            var transform = runtimeObject.Transform;
            var rotationDelta = (float)context.DeltaSeconds * kDefaultRotationRadiansPerSecond;
            var updatedRotation = Quaternion.Normalize(
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, rotationDelta) * transform.LocalRotation);
            transform.SetLocalTransform(transform.LocalPosition, updatedRotation, transform.LocalScale);
            return;
        }
    }
}
