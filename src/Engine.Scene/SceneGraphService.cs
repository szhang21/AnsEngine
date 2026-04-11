using Engine.Core;

namespace Engine.Scene;

public sealed class SceneGraphService : ISceneRenderContractProvider
{
    private const string DefaultMeshId = "mesh://triangle";
    private const string DefaultMaterialId = "material://default";
    private const string AnimatedMaterialId = "material://pulse";

    private readonly EngineRuntimeInfo _runtimeInfo;
    private readonly List<SceneRenderItem> _renderItems = [];
    private int _nextNodeId = 1;
    private int _frameNumber;

    public SceneGraphService(EngineRuntimeInfo runtimeInfo)
    {
        _runtimeInfo = runtimeInfo;
    }

    public int NodeCount { get; private set; }

    public void AddRootNode()
    {
        _ = _runtimeInfo.EngineName;

        var nodeId = _nextNodeId;
        _nextNodeId += 1;
        NodeCount += 1;
        _renderItems.Add(new SceneRenderItem(nodeId, DefaultMeshId, DefaultMaterialId));
    }

    public SceneRenderFrame BuildRenderFrame()
    {
        if (_renderItems.Count > 0)
        {
            var currentMaterialId = (_frameNumber & 1) == 0 ? DefaultMaterialId : AnimatedMaterialId;
            var firstItem = _renderItems[0];
            _renderItems[0] = firstItem with
            {
                MaterialId = currentMaterialId
            };
        }

        var itemsSnapshot = _renderItems.ToArray();
        var frame = new SceneRenderFrame(_frameNumber, itemsSnapshot);
        _frameNumber += 1;
        return frame;
    }
}
