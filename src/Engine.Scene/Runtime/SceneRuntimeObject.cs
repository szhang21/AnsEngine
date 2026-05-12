namespace Engine.Scene;

using Engine.Runtime.Abstractions;

internal sealed class SceneRuntimeObject : IRuntimeObject
{
    private readonly IReadOnlyList<IRuntimeComponent> mComponents;

    public SceneRuntimeObject(
        int nodeId,
        string objectId,
        string objectName,
        SceneTransformComponent? transform = null,
        SceneMeshRendererComponent? meshRenderer = null,
        IEnumerable<IRuntimeComponent>? components = null)
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
        mComponents = CreateComponentCollection(meshRenderer, components);
    }

    public int NodeId { get; }

    public string ObjectId { get; }

    public string ObjectName { get; }

    public SceneTransformComponent? Transform { get; }

    public SceneMeshRendererComponent? MeshRenderer => GetComponent<SceneMeshRendererComponent>();

    public T? GetComponent<T>() where T : class, IRuntimeComponent
    {
        for (var index = 0; index < mComponents.Count; index += 1)
        {
            if (mComponents[index] is T component)
            {
                return component;
            }
        }

        return null;
    }

    public bool HasComponent<T>() where T : class, IRuntimeComponent
    {
        return GetComponent<T>() is not null;
    }

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

    private static IReadOnlyList<IRuntimeComponent> CreateComponentCollection(
        SceneMeshRendererComponent? meshRenderer,
        IEnumerable<IRuntimeComponent>? components)
    {
        if (meshRenderer is null)
        {
            return components?.ToArray() ?? Array.Empty<IRuntimeComponent>();
        }

        if (components is null)
        {
            return new IRuntimeComponent[] { meshRenderer };
        }

        return new IRuntimeComponent[] { meshRenderer }.Concat(components).ToArray();
    }
}
