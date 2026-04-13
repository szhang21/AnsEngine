namespace Engine.Scene;

public readonly record struct SceneRenderItem(int NodeId, string MeshId, string MaterialId);

public sealed record SceneRenderFrame(int FrameNumber, IReadOnlyList<SceneRenderItem> Items)
{
    public static SceneRenderFrame FromContracts(Engine.Contracts.SceneRenderFrame frame)
    {
        var items = frame.Items
            .Select(item => new SceneRenderItem(item.NodeId, item.MeshId, item.MaterialId))
            .ToArray();
        return new SceneRenderFrame(frame.FrameNumber, items);
    }
}

public interface ISceneRenderContractProvider
{
    SceneRenderFrame BuildRenderFrame();
}
