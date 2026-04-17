using System.Numerics;

namespace Engine.Contracts;

public readonly record struct SceneCamera(Matrix4x4 View, Matrix4x4 Projection)
{
    public static SceneCamera Identity { get; } = new(Matrix4x4.Identity, Matrix4x4.Identity);
}

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

public sealed record SceneRenderFrame
{
    public SceneRenderFrame(int frameNumber, IReadOnlyList<SceneRenderItem> items)
        : this(frameNumber, items, SceneCamera.Identity)
    {
    }

    public SceneRenderFrame(int frameNumber, IReadOnlyList<SceneRenderItem> items, SceneCamera camera)
    {
        FrameNumber = frameNumber;
        Items = items;
        Camera = camera;
    }

    public int FrameNumber { get; }

    public IReadOnlyList<SceneRenderItem> Items { get; }

    public SceneCamera Camera { get; }
}

public interface ISceneRenderContractProvider
{
    SceneRenderFrame BuildRenderFrame();
}
