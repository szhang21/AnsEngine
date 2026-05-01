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
    public void SceneRuntimeObject_StoresIdentityFields()
    {
        var runtimeObject = new SceneRuntimeObject(7, "cube-main", "Cube Main");

        Assert.Equal(7, runtimeObject.NodeId);
        Assert.Equal("cube-main", runtimeObject.ObjectId);
        Assert.Equal("Cube Main", runtimeObject.ObjectName);
    }

    [Fact]
    public void RuntimeScene_CreateAndClear_TracksObjectCount()
    {
        var runtimeScene = new RuntimeScene();

        runtimeScene.CreateObject(1, "cube-main", "Cube Main");
        runtimeScene.CreateObject(2, "cube-secondary", "Cube Secondary");

        Assert.Equal(2, runtimeScene.ObjectCount);
        Assert.Equal("cube-main", runtimeScene.Objects[0].ObjectId);
        Assert.Equal("Cube Secondary", runtimeScene.Objects[1].ObjectName);

        runtimeScene.Clear();

        Assert.Equal(0, runtimeScene.ObjectCount);
        Assert.Empty(runtimeScene.Objects);
    }

    [Fact]
    public void SceneTransformComponent_FromDescription_MapsToContractsTransform()
    {
        var rotation = Quaternion.CreateFromYawPitchRoll(0.1f, 0.2f, 0.3f);
        var transformDescription = new SceneTransformDescription(
            new Vector3(1.0f, 2.0f, 3.0f),
            rotation,
            new Vector3(2.0f, 3.0f, 4.0f));

        var component = SceneTransformComponent.FromDescription(transformDescription);
        var transform = component.ToSceneTransform();

        Assert.Equal(new Vector3(1.0f, 2.0f, 3.0f), component.LocalPosition);
        Assert.Equal(rotation, component.LocalRotation);
        Assert.Equal(new Vector3(2.0f, 3.0f, 4.0f), component.LocalScale);
        Assert.Equal(component.LocalPosition, transform.Position);
        Assert.Equal(component.LocalRotation, transform.Rotation);
        Assert.Equal(component.LocalScale, transform.Scale);
    }

    [Fact]
    public void SceneMeshRendererComponent_FromDescription_MapsResourceReferences()
    {
        var objectDescription = new SceneObjectDescription(
            "cube-main",
            "Cube Main",
            new Engine.Contracts.SceneMeshRef("mesh://cube"),
            new Engine.Contracts.SceneMaterialRef("material://highlight"),
            SceneTransformDescription.Identity);

        var component = SceneMeshRendererComponent.FromDescription(objectDescription);

        Assert.Equal("mesh://cube", component.Mesh.MeshId);
        Assert.Equal("material://highlight", component.Material.MaterialId);
    }

    [Fact]
    public void SceneRuntimeObject_CanHoldTransformAndMeshRendererComponents()
    {
        var transform = SceneTransformComponent.FromDescription(SceneTransformDescription.Identity);
        var meshRenderer = new SceneMeshRendererComponent(
            new Engine.Contracts.SceneMeshRef("mesh://cube"),
            new Engine.Contracts.SceneMaterialRef("material://default"));

        var runtimeObject = new SceneRuntimeObject(1, "cube-main", "Cube Main", transform, meshRenderer);

        Assert.Same(transform, runtimeObject.Transform);
        Assert.Same(meshRenderer, runtimeObject.MeshRenderer);
    }

    [Fact]
    public void SceneCameraRuntimeState_FromDescription_BuildsDescriptionCamera()
    {
        var cameraDescription = new SceneCameraDescription(
            new Vector3(1.0f, 2.0f, 3.0f),
            Vector3.Zero,
            0.75f);

        var cameraState = SceneCameraRuntimeState.FromDescription(cameraDescription);
        var camera = cameraState.BuildCamera(16.0f / 9.0f, 0.1f, 10.0f);

        Assert.Equal(cameraDescription.Position, cameraState.Position);
        Assert.Equal(cameraDescription.Target, cameraState.Target);
        Assert.Equal(cameraDescription.FieldOfViewRadians, cameraState.FieldOfViewRadians);
        AssertValidCamera(camera);
    }

    [Fact]
    public void SceneCameraRuntimeState_DefaultCamera_MatchesSceneGraphDefaultCamera()
    {
        var cameraState = SceneCameraRuntimeState.FromDescription(null);
        var expectedCamera = cameraState.BuildCamera(16.0f / 9.0f, 0.1f, 10.0f);
        var sceneGraph = new SceneGraphService(new EngineRuntimeInfo("AnsEngine", "0.1.0"));

        var frame = sceneGraph.BuildRenderFrame();

        Assert.Equal(expectedCamera, frame.Camera);
    }

    [Fact]
    public void LoadSceneDescription_ReplacesRuntimeSceneObjectCount()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        sceneGraph.AddRootNode();
        sceneGraph.AddRootNode();

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

        Assert.Equal(1, sceneGraph.NodeCount);
    }

    [Fact]
    public void RuntimeScene_LoadFromDescription_MapsObjectsComponentsAndCamera()
    {
        var runtimeScene = new RuntimeScene();
        var rotation = Quaternion.CreateFromYawPitchRoll(0.1f, 0.2f, 0.3f);
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
                    new SceneTransformDescription(new Vector3(1, 2, 3), rotation, new Vector3(2, 2, 2))),
                new SceneObjectDescription(
                    "cube-secondary",
                    "Cube Secondary",
                    new Engine.Contracts.SceneMeshRef("mesh://missing"),
                    new Engine.Contracts.SceneMaterialRef("material://default"),
                    SceneTransformDescription.Identity)
            });

        runtimeScene.LoadFromDescription(description);

        Assert.Equal(2, runtimeScene.ObjectCount);
        Assert.Equal(1, runtimeScene.Objects[0].NodeId);
        Assert.Equal(2, runtimeScene.Objects[1].NodeId);
        Assert.Equal("cube-main", runtimeScene.Objects[0].ObjectId);
        Assert.Equal("Cube Secondary", runtimeScene.Objects[1].ObjectName);
        Assert.Equal(new Vector3(1, 2, 3), runtimeScene.Objects[0].Transform!.LocalPosition);
        Assert.Equal(rotation, runtimeScene.Objects[0].Transform!.LocalRotation);
        Assert.Equal("mesh://cube", runtimeScene.Objects[0].MeshRenderer!.Mesh.MeshId);
        Assert.Equal("material://highlight", runtimeScene.Objects[0].MeshRenderer!.Material.MaterialId);
        Assert.Equal(description.Camera.Position, runtimeScene.Camera.Position);
    }

    [Fact]
    public void RuntimeScene_LoadFromDescription_EmptySceneClearsPreviousObjects()
    {
        var runtimeScene = new RuntimeScene();
        runtimeScene.CreateObject(1, "old-object", "Old Object");

        runtimeScene.LoadFromDescription(
            new SceneDescription(
                "empty-scene",
                "Empty Scene",
                null!,
                Array.Empty<SceneObjectDescription>()));

        Assert.Equal(0, runtimeScene.ObjectCount);
        Assert.Empty(runtimeScene.Objects);
        Assert.Equal(SceneCameraRuntimeState.CreateDefault().Position, runtimeScene.Camera.Position);
    }

    [Fact]
    public void RuntimeScene_BuildRenderItems_OutputsOnlyObjectsWithRenderableComponents()
    {
        var runtimeScene = new RuntimeScene();
        runtimeScene.CreateObject(
            1,
            "cube-main",
            "Cube Main",
            SceneTransformComponent.FromDescription(SceneTransformDescription.Identity),
            new SceneMeshRendererComponent(
                new Engine.Contracts.SceneMeshRef("mesh://cube"),
                new Engine.Contracts.SceneMaterialRef("material://default")));
        runtimeScene.CreateObject(2, "empty-object", "Empty Object");

        var items = runtimeScene.BuildRenderItems();

        var item = Assert.Single(items);
        Assert.Equal(1, item.NodeId);
        Assert.Equal("mesh://cube", item.MeshId);
        Assert.Equal("material://default", item.MaterialId);
        Assert.Equal(SceneTransform.Identity, item.Transform);
    }

    [Fact]
    public void AddRootNode_AfterSceneDescriptionLoad_AddsToRuntimeScene()
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
                        SceneTransformDescription.Identity),
                    new SceneObjectDescription(
                        "cube-secondary",
                        "Cube Secondary",
                        new Engine.Contracts.SceneMeshRef("mesh://missing"),
                        new Engine.Contracts.SceneMaterialRef("material://default"),
                        SceneTransformDescription.Identity)
                }));

        sceneGraph.AddRootNode();

        Assert.Equal(3, sceneGraph.NodeCount);
        Assert.Equal(new[] { 1, 2, 3 }, sceneGraph.BuildRenderFrame().Items.Select(item => item.NodeId));
        Assert.Equal("node-3", sceneGraph.FindObject("node-3")!.ObjectId);
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
    public void BuildRenderFrame_ConsecutiveFrames_PreserveRuntimeStateAndIncrementFrameNumber()
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
        Assert.Equal("material://default", secondItem.MaterialId);
        Assert.Equal(0, firstFrame.FrameNumber);
        Assert.Equal(1, secondFrame.FrameNumber);

        Assert.Equal(Vector3.Zero, firstItem.Transform.Position);
        Assert.Equal(Quaternion.Identity, firstItem.Transform.Rotation);
        Assert.Equal(Vector3.Zero, secondItem.Transform.Position);
        Assert.Equal(Quaternion.Identity, secondItem.Transform.Rotation);
        Assert.Equal(Vector3.One, firstItem.Transform.Scale);
        Assert.Equal(Vector3.One, secondItem.Transform.Scale);
        Assert.Equal(firstItem.Transform, secondItem.Transform);
        Assert.Equal(firstFrame.Camera.View, secondFrame.Camera.View);
        Assert.Equal(firstFrame.Camera.Projection, secondFrame.Camera.Projection);

        AssertValidTransform(secondItem.Transform.Position, secondItem.Transform.Rotation);
        AssertValidCamera(secondFrame.Camera);
    }

    [Fact]
    public void BuildRenderFrame_ConsecutiveFrames_UseRuntimeMaterialId()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        sceneGraph.AddRootNode();

        var frame0 = sceneGraph.BuildRenderFrame();
        var frame1 = sceneGraph.BuildRenderFrame();
        var frame2 = sceneGraph.BuildRenderFrame();
        var frame3 = sceneGraph.BuildRenderFrame();

        Assert.Equal("material://default", Assert.Single(frame0.Items).MaterialId);
        Assert.Equal("material://default", Assert.Single(frame1.Items).MaterialId);
        Assert.Equal("material://default", Assert.Single(frame2.Items).MaterialId);
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
    public void BuildRenderFrame_AfterRuntimeTransformChange_UsesRuntimeComponentValue()
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
        sceneGraph.RuntimeScene.Objects[0].Transform!.SetLocalTransform(
            new Vector3(4.0f, 5.0f, 6.0f),
            Quaternion.Identity,
            new Vector3(2.0f, 2.0f, 2.0f));

        var frame = sceneGraph.BuildRenderFrame();

        var item = Assert.Single(frame.Items);
        Assert.Equal(new Vector3(4.0f, 5.0f, 6.0f), item.Transform.Position);
        Assert.Equal(new Vector3(2.0f, 2.0f, 2.0f), item.Transform.Scale);
    }

    [Fact]
    public void CreateRuntimeSnapshot_LoadedScene_ReturnsReadOnlyObjectAndCameraValues()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        var cameraPosition = new Vector3(0.0f, 0.25f, 2.2f);
        sceneGraph.LoadSceneDescription(
            new SceneDescription(
                "sample-scene",
                "Sample Scene",
                new SceneCameraDescription(cameraPosition, Vector3.Zero, 1.0471976f),
                new[]
                {
                    new SceneObjectDescription(
                        "cube-main",
                        "Cube Main",
                        new Engine.Contracts.SceneMeshRef("mesh://cube"),
                        new Engine.Contracts.SceneMaterialRef("material://highlight"),
                        new SceneTransformDescription(new Vector3(1, 2, 3), Quaternion.Identity, new Vector3(2, 2, 2)))
                }));

        var snapshot = sceneGraph.CreateRuntimeSnapshot();

        Assert.Equal(1, snapshot.ObjectCount);
        Assert.Equal(cameraPosition, snapshot.Camera.Position);
        var item = Assert.Single(snapshot.Objects);
        Assert.Equal(1, item.NodeId);
        Assert.Equal("cube-main", item.ObjectId);
        Assert.Equal("Cube Main", item.ObjectName);
        Assert.True(item.HasTransform);
        Assert.Equal(new Vector3(1, 2, 3), item.LocalTransform!.Value.Position);
        Assert.True(item.HasMeshRenderer);
        Assert.Equal("mesh://cube", item.Mesh!.Value.MeshId);
        Assert.Equal("material://highlight", item.Material!.Value.MaterialId);
    }

    [Fact]
    public void FindObject_ExistingObject_ReturnsSnapshot()
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

        var snapshot = sceneGraph.FindObject("cube-main");

        Assert.NotNull(snapshot);
        Assert.Equal("cube-main", snapshot!.ObjectId);
        Assert.Null(sceneGraph.FindObject("missing-object"));
    }

    [Fact]
    public void CreateRuntimeSnapshot_DoesNotExposeMutableRuntimeState()
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

        var snapshot = sceneGraph.CreateRuntimeSnapshot();
        sceneGraph.RuntimeScene.Objects[0].Transform!.SetLocalTransform(
            new Vector3(9, 9, 9),
            Quaternion.Identity,
            Vector3.One);

        Assert.Equal(Vector3.Zero, Assert.Single(snapshot.Objects).LocalTransform!.Value.Position);
        Assert.Equal(new Vector3(9, 9, 9), sceneGraph.FindObject("cube-main")!.LocalTransform!.Value.Position);
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
        var thirdFrame = sceneGraph.BuildRenderFrame();

        Assert.Equal(0, firstFrame.FrameNumber);
        Assert.Equal(1, secondFrame.FrameNumber);
        Assert.Equal(2, thirdFrame.FrameNumber);
        Assert.Equal(firstFrame.Items, secondFrame.Items);
        Assert.Equal(firstFrame.Items, thirdFrame.Items);
        Assert.Equal(firstFrame.Camera, secondFrame.Camera);
        Assert.Equal(firstFrame.Camera, thirdFrame.Camera);
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
