using System.Numerics;

namespace Engine.Contracts;

public readonly record struct SceneTransform(Vector3 Position, Vector3 Scale, Quaternion Rotation)
{
    public static SceneTransform Identity { get; } = new(Vector3.Zero, Vector3.One, Quaternion.Identity);
}

public readonly record struct SceneRenderItem
{
    public SceneRenderItem(int nodeId, string meshId, string materialId)
        : this(nodeId, meshId, materialId, SceneTransform.Identity)
    {
    }

    public SceneRenderItem(int nodeId, string meshId, string materialId, SceneTransform transform)
    {
        NodeId = nodeId;
        MeshId = meshId;
        MaterialId = materialId;
        Transform = transform;
    }

    public int NodeId { get; init; }

    public string MeshId { get; init; }

    public string MaterialId { get; init; }

    public SceneTransform Transform { get; init; }
}

public sealed record SceneRenderFrame(int FrameNumber, IReadOnlyList<SceneRenderItem> Items);

public interface ISceneRenderContractProvider
{
    SceneRenderFrame BuildRenderFrame();
}
