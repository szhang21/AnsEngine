namespace Engine.Contracts;

public readonly record struct SceneRenderItem(int NodeId, string MeshId, string MaterialId);

public sealed record SceneRenderFrame(int FrameNumber, IReadOnlyList<SceneRenderItem> Items);

public interface ISceneRenderContractProvider
{
    SceneRenderFrame BuildRenderFrame();
}
