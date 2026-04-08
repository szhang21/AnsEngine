using Engine.Core;

namespace Engine.Scene;

public sealed class SceneGraphService
{
    private readonly EngineRuntimeInfo _runtimeInfo;

    public SceneGraphService(EngineRuntimeInfo runtimeInfo)
    {
        _runtimeInfo = runtimeInfo;
    }

    public int NodeCount { get; private set; }

    public void AddRootNode()
    {
        _ = _runtimeInfo.EngineName;
        NodeCount += 1;
    }
}
