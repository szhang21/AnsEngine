namespace Engine.Scene;

internal sealed class SceneRuntimeObject
{
    public SceneRuntimeObject(
        int nodeId,
        string objectId,
        string objectName,
        SceneTransformComponent? transform = null,
        SceneMeshRendererComponent? meshRenderer = null)
    {
        if (nodeId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(nodeId), "NodeId must be positive.");
        }

        NodeId = nodeId;
        ObjectId = string.IsNullOrWhiteSpace(objectId)
            ? throw new ArgumentException("ObjectId must not be null or whitespace.", nameof(objectId))
            : objectId;
        ObjectName = string.IsNullOrWhiteSpace(objectName) ? ObjectId : objectName;
        Transform = transform;
        MeshRenderer = meshRenderer;
    }

    public int NodeId { get; }

    public string ObjectId { get; }

    public string ObjectName { get; }

    public SceneTransformComponent? Transform { get; }

    public SceneMeshRendererComponent? MeshRenderer { get; }

    public SceneRuntimeObjectSnapshot CreateSnapshot()
    {
        return new SceneRuntimeObjectSnapshot(
            NodeId,
            ObjectId,
            ObjectName,
            Transform is not null,
            Transform?.ToSceneTransform(),
            MeshRenderer is not null,
            MeshRenderer?.Mesh,
            MeshRenderer?.Material);
    }
}
