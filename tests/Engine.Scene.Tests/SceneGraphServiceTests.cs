using Engine.Core;
using Engine.Scene;
using Engine.SceneData;
using Engine.Contracts;
using System.Numerics;
using ContractsProvider = Engine.Contracts.ISceneRenderContractProvider;

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
        AssertValidCamera(frame.Camera);
    }

    [Fact]
    public void AddRootNode_BuildRenderFrame_ReturnsDefaultRenderSubmissionWithIdentityTransform()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        sceneGraph.AddRootNode();

        var frame = sceneGraph.BuildRenderFrame();

        var item = Assert.Single(frame.Items);
        Assert.Equal(1, item.NodeId);
        Assert.Equal("mesh://cube", item.MeshId);
        Assert.Equal("material://default", item.MaterialId);
        Assert.Equal(Vector3.Zero, item.Transform.Position);
        Assert.Equal(Vector3.One, item.Transform.Scale);
        Assert.Equal(Quaternion.Identity, item.Transform.Rotation);
        AssertValidCamera(frame.Camera);
    }

    [Fact]
    public void BuildRenderFrame_ConsecutiveFrames_TransformAndMaterialChangeAndRemainValid()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        sceneGraph.AddRootNode();

        var firstFrame = sceneGraph.BuildRenderFrame();
        var secondFrame = sceneGraph.BuildRenderFrame();

        var firstItem = Assert.Single(firstFrame.Items);
        var secondItem = Assert.Single(secondFrame.Items);

        Assert.Equal(1, firstItem.NodeId);
        Assert.Equal(1, secondItem.NodeId);
        Assert.Equal("material://default", firstItem.MaterialId);
        Assert.Equal("material://pulse", secondItem.MaterialId);
        Assert.Equal(0, firstFrame.FrameNumber);
        Assert.Equal(1, secondFrame.FrameNumber);

        Assert.Equal(Vector3.Zero, firstItem.Transform.Position);
        Assert.Equal(Quaternion.Identity, firstItem.Transform.Rotation);
        Assert.Equal(new Vector3(0.00005f, 0.0f, 0.0f), secondItem.Transform.Position);
        Assert.Equal(Quaternion.CreateFromYawPitchRoll(0.005f, 0.003f, 0.0f), secondItem.Transform.Rotation);
        Assert.Equal(Vector3.One, firstItem.Transform.Scale);
        Assert.Equal(Vector3.One, secondItem.Transform.Scale);
        Assert.NotEqual(firstItem.Transform, secondItem.Transform);
        Assert.NotEqual(firstFrame.Camera.View, secondFrame.Camera.View);
        Assert.Equal(firstFrame.Camera.Projection, secondFrame.Camera.Projection);

        AssertValidTransform(secondItem.Transform.Position, secondItem.Transform.Rotation);
        AssertValidCamera(secondFrame.Camera);
    }

    [Fact]
    public void BuildRenderFrame_MaterialCycle_UsesRenderAlignedMaterialIdsAndFallback()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        sceneGraph.AddRootNode();

        var frame0 = sceneGraph.BuildRenderFrame();
        var frame1 = sceneGraph.BuildRenderFrame();
        var frame2 = sceneGraph.BuildRenderFrame();
        var frame3 = sceneGraph.BuildRenderFrame();

        Assert.Equal("material://default", Assert.Single(frame0.Items).MaterialId);
        Assert.Equal("material://pulse", Assert.Single(frame1.Items).MaterialId);
        Assert.Equal("material://highlight", Assert.Single(frame2.Items).MaterialId);
        Assert.Equal("material://default", Assert.Single(frame3.Items).MaterialId);
    }

    [Fact]
    public void BuildRenderFrame_MultipleNodes_PreservesSharedAndMissingMeshReferences()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        sceneGraph.AddRootNode();
        sceneGraph.AddRootNode();
        sceneGraph.AddRootNode();

        var frame = sceneGraph.BuildRenderFrame();

        Assert.Equal(3, frame.Items.Count);
        Assert.Equal("mesh://cube", frame.Items[0].MeshId);
        Assert.Equal("mesh://missing", frame.Items[1].MeshId);
        Assert.Equal("mesh://cube", frame.Items[2].MeshId);
        Assert.Equal(frame.Items[0].MeshId, frame.Items[2].MeshId);
    }

    [Fact]
    public void BuildRenderFrame_FromContractsInterface_ExposesMissingMeshReferenceForDownstreamFallback()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        sceneGraph.AddRootNode();
        sceneGraph.AddRootNode();
        ContractsProvider contractsProvider = sceneGraph;

        var frame = contractsProvider.BuildRenderFrame();

        Assert.Equal(2, frame.Items.Count);
        Assert.Equal("mesh://cube", frame.Items[0].MeshId);
        Assert.Equal("mesh://missing", frame.Items[1].MeshId);
    }

    [Fact]
    public void BuildRenderFrame_FromContractsInterface_ReturnsContractsFrameWithIdentityTransform()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        sceneGraph.AddRootNode();
        ContractsProvider contractsProvider = sceneGraph;

        var frame = contractsProvider.BuildRenderFrame();

        var item = Assert.Single(frame.Items);
        Assert.Equal(1, item.NodeId);
        Assert.Equal("mesh://cube", item.MeshId);
        Assert.Equal("material://default", item.MaterialId);
        Assert.Equal(Vector3.Zero, item.Transform.Position);
        Assert.Equal(Vector3.One, item.Transform.Scale);
        Assert.Equal(Quaternion.Identity, item.Transform.Rotation);
        AssertValidCamera(frame.Camera);
    }

    [Fact]
    public void LoadSceneDescription_BuildRenderFrame_MapsObjectsCameraAndLocalTransforms()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        var description = new SceneDescription(
            "sample-scene",
            "Sample Scene",
            new SceneCameraDescription(new Vector3(0.0f, 0.25f, 2.2f), Vector3.Zero, 1.0471976f),
            new[]
            {
                new SceneObjectDescription(
                    "cube-main",
                    "Cube Main",
                    new Engine.Contracts.SceneMeshRef("mesh://cube"),
                    new Engine.Contracts.SceneMaterialRef("material://highlight"),
                    new SceneTransformDescription(
                        new Vector3(1.0f, 2.0f, 3.0f),
                        Quaternion.CreateFromYawPitchRoll(0.1f, 0.2f, 0.3f),
                        new Vector3(2.0f, 2.0f, 2.0f))),
                new SceneObjectDescription(
                    "cube-secondary",
                    "Cube Secondary",
                    new Engine.Contracts.SceneMeshRef("mesh://missing"),
                    new Engine.Contracts.SceneMaterialRef("material://default"),
                    SceneTransformDescription.Identity)
            });

        sceneGraph.LoadSceneDescription(description);

        var frame = sceneGraph.BuildRenderFrame();

        Assert.Equal(0, frame.FrameNumber);
        Assert.Equal(2, frame.Items.Count);

        var first = frame.Items[0];
        Assert.Equal(1, first.NodeId);
        Assert.Equal("mesh://cube", first.MeshId);
        Assert.Equal("material://highlight", first.MaterialId);
        Assert.Equal(new Vector3(1.0f, 2.0f, 3.0f), first.Transform.Position);
        Assert.Equal(new Vector3(2.0f, 2.0f, 2.0f), first.Transform.Scale);
        Assert.Equal(Quaternion.CreateFromYawPitchRoll(0.1f, 0.2f, 0.3f), first.Transform.Rotation);

        var second = frame.Items[1];
        Assert.Equal(2, second.NodeId);
        Assert.Equal("mesh://missing", second.MeshId);
        Assert.Equal("material://default", second.MaterialId);
        Assert.Equal(SceneTransform.Identity, second.Transform);

        AssertValidCamera(frame.Camera);
    }

    [Fact]
    public void LoadSceneDescription_ConsecutiveFrames_PreserveDescriptionDrivenOutputs()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        sceneGraph.LoadSceneDescription(
            new SceneDescription(
                "sample-scene",
                "Sample Scene",
                new SceneCameraDescription(new Vector3(0.0f, 0.0f, 2.2f), Vector3.Zero, 1.0471976f),
                new[]
                {
                    new SceneObjectDescription(
                        "cube-main",
                        "Cube Main",
                        new Engine.Contracts.SceneMeshRef("mesh://cube"),
                        new Engine.Contracts.SceneMaterialRef("material://default"),
                        SceneTransformDescription.Identity)
                }));

        var firstFrame = sceneGraph.BuildRenderFrame();
        var secondFrame = sceneGraph.BuildRenderFrame();

        Assert.Equal(0, firstFrame.FrameNumber);
        Assert.Equal(1, secondFrame.FrameNumber);
        Assert.Equal(firstFrame.Items, secondFrame.Items);
        Assert.Equal(firstFrame.Camera, secondFrame.Camera);
    }

    [Fact]
    public void LoadSceneDescription_NullCamera_UsesDefaultSceneCamera()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        sceneGraph.LoadSceneDescription(
            new SceneDescription(
                "sample-scene",
                "Sample Scene",
                null!,
                new[]
                {
                    new SceneObjectDescription(
                        "cube-main",
                        "Cube Main",
                        new Engine.Contracts.SceneMeshRef("mesh://cube"),
                        new Engine.Contracts.SceneMaterialRef("material://default"),
                        SceneTransformDescription.Identity)
                }));

        var frame = sceneGraph.BuildRenderFrame();

        AssertValidCamera(frame.Camera);
        Assert.NotEqual(Matrix4x4.Identity, frame.Camera.View);
        Assert.NotEqual(Matrix4x4.Identity, frame.Camera.Projection);
    }

    private static void AssertValidTransform(Vector3 position, Quaternion rotation)
    {
        Assert.False(float.IsNaN(position.X));
        Assert.False(float.IsNaN(position.Y));
        Assert.False(float.IsNaN(position.Z));
        Assert.False(float.IsNaN(rotation.X));
        Assert.False(float.IsNaN(rotation.Y));
        Assert.False(float.IsNaN(rotation.Z));
        Assert.False(float.IsNaN(rotation.W));
    }

    private static void AssertValidCamera(Engine.Contracts.SceneCamera camera)
    {
        Assert.False(float.IsNaN(camera.View.M11));
        Assert.False(float.IsNaN(camera.View.M22));
        Assert.False(float.IsNaN(camera.Projection.M11));
        Assert.False(float.IsNaN(camera.Projection.M22));
        Assert.NotEqual(Matrix4x4.Identity, camera.View);
        Assert.NotEqual(Matrix4x4.Identity, camera.Projection);
    }
}
