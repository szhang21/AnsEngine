using Engine.Core;
using Engine.Contracts;

namespace Engine.Scene;

public sealed class SceneGraphService : ISceneRenderContractProvider
{
    private const string DefaultMeshId = "mesh://triangle";
    private const string DefaultMaterialId = "material://default";
    private const string AnimatedMaterialId = "material://pulse";

    private readonly EngineRuntimeInfo runtimeInfo;
    private readonly List<SceneRenderItem> renderItems = [];
    private int nextNodeId = 1;
    private int frameNumber;

    public SceneGraphService(EngineRuntimeInfo runtimeInfo)
    {
        this.runtimeInfo = runtimeInfo;
    }

    public int NodeCount { get; private set; }

    public void AddRootNode()
    {
        _ = runtimeInfo.EngineName;

        var nodeId = nextNodeId;
        nextNodeId += 1;
        NodeCount += 1;
        renderItems.Add(new SceneRenderItem(nodeId, DefaultMeshId, DefaultMaterialId));
    }

    public SceneRenderFrame BuildRenderFrame()
    {
        if (renderItems.Count > 0)
        {
            var currentMaterialId = (frameNumber & 1) == 0 ? DefaultMaterialId : AnimatedMaterialId;
            var firstItem = renderItems[0];
            renderItems[0] = firstItem with
            {
                MaterialId = currentMaterialId
            };
        }

        var itemsSnapshot = renderItems.ToArray();
        var frame = new SceneRenderFrame(frameNumber, itemsSnapshot);
        frameNumber += 1;
        return frame;
    }
}
