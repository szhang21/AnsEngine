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

    [Fact]
    public void BuildRenderFrame_EmptyScene_ReturnsEmptyItems()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);

        var frame = sceneGraph.BuildRenderFrame();

        Assert.Equal(0, frame.FrameNumber);
        Assert.Empty(frame.Items);
    }

    [Fact]
    public void AddRootNode_BuildRenderFrame_ReturnsDefaultRenderSubmission()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        sceneGraph.AddRootNode();

        var frame = sceneGraph.BuildRenderFrame();

        var item = Assert.Single(frame.Items);
        Assert.Equal(1, item.NodeId);
        Assert.Equal("mesh://triangle", item.MeshId);
        Assert.Equal("material://default", item.MaterialId);
    }

    [Fact]
    public void BuildRenderFrame_ConsecutiveFrames_ToggleFirstNodeMaterial()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        sceneGraph.AddRootNode();

        var firstFrame = sceneGraph.BuildRenderFrame();
        var secondFrame = sceneGraph.BuildRenderFrame();

        var firstItem = Assert.Single(firstFrame.Items);
        var secondItem = Assert.Single(secondFrame.Items);

        Assert.Equal("material://default", firstItem.MaterialId);
        Assert.Equal("material://pulse", secondItem.MaterialId);
        Assert.NotEqual(firstItem.MaterialId, secondItem.MaterialId);
        Assert.Equal(0, firstFrame.FrameNumber);
        Assert.Equal(1, secondFrame.FrameNumber);
    }
}
