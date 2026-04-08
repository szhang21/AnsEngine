using Engine.Core;
using Engine.Scene;

namespace Engine.Scene.Tests;

public sealed class SceneGraphServiceTests
{
    [Fact]
    public void AddRootNode_IncrementsNodeCount()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);

        sceneGraph.AddRootNode();

        Assert.Equal(1, sceneGraph.NodeCount);
    }
}
