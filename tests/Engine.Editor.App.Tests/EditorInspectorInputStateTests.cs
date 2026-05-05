using System.Numerics;
using Engine.Editor.App;
using Engine.SceneData;
using Xunit;

namespace Engine.Editor.App.Tests;

public sealed class EditorInspectorInputStateTests
{
    [Fact]
    public void SyncFrom_SameSelectedObject_PreservesPendingTransformInput()
    {
        var controller = CreateControllerWithSelectedObject();
        var snapshot = EditorGuiSnapshotFactory.Create(controller);
        var inputState = new EditorInspectorInputState();
        inputState.SyncFrom(snapshot.Inspector);
        var pendingPosition = new Vector3(9, 8, 7);
        var pendingScale = new Vector3(3, 3, 3);
        inputState.SetTransformValues(pendingPosition, Quaternion.Identity, pendingScale);

        var nextFrameSnapshot = EditorGuiSnapshotFactory.Create(controller);
        inputState.SyncFrom(nextFrameSnapshot.Inspector);

        Assert.Equal(pendingPosition, inputState.Position);
        Assert.Equal(pendingScale, inputState.Scale);
        Assert.Equal(snapshot.Inspector.ObjectId, inputState.ObjectId);
    }

    [Fact]
    public void SyncFrom_DifferentSelectedObject_LoadsNewObjectValues()
    {
        var controller = CreateControllerWithSelectedObject();
        var snapshot = EditorGuiSnapshotFactory.Create(controller);
        var inputState = new EditorInspectorInputState();
        inputState.SyncFrom(snapshot.Inspector);
        inputState.SetTextValues(snapshot.Inspector.ObjectId, "Pending Edit", snapshot.Inspector.Mesh, snapshot.Inspector.Material);
        inputState.SetTransformValues(new Vector3(9, 8, 7), Quaternion.Identity, new Vector3(3, 3, 3));

        Assert.True(controller.SelectObject("cube-b"), controller.LastError);
        var nextObjectSnapshot = EditorGuiSnapshotFactory.Create(controller);
        inputState.SyncFrom(nextObjectSnapshot.Inspector);

        Assert.Equal("cube-b", inputState.ObjectId);
        Assert.Equal("Cube B", inputState.ObjectName);
        Assert.Equal(nextObjectSnapshot.Inspector.Position, inputState.Position);
        Assert.Equal(nextObjectSnapshot.Inspector.Scale, inputState.Scale);
        Assert.True(inputState.HasMeshRenderer);
    }

    [Fact]
    public void Apply_NameAndTransform_UpdatesSessionAndDirty()
    {
        var controller = CreateControllerWithSelectedObject();
        var snapshot = EditorGuiSnapshotFactory.Create(controller);
        var inputState = new EditorInspectorInputState();
        inputState.SyncFrom(snapshot.Inspector);
        inputState.SetTextValues(snapshot.Inspector.ObjectId, "Edited Cube", snapshot.Inspector.Mesh, snapshot.Inspector.Material);
        inputState.SetTransformValues(new Vector3(1, 2, 3), Quaternion.Identity, new Vector3(2, 2, 2));

        var result = inputState.Apply(controller, snapshot.Inspector);

        Assert.True(result, controller.LastError);
        Assert.True(controller.Session.IsDirty);
        Assert.Equal("Edited Cube", controller.Session.SelectedObject!.ObjectName);
        Assert.Equal(new Vector3(1, 2, 3), controller.Session.SelectedObject.LocalTransform.Position);
        Assert.Equal(new Vector3(2, 2, 2), controller.Session.SelectedObject.LocalTransform.Scale);
        Assert.Equal("mesh://cube", controller.Session.SelectedObject.MeshRendererComponent!.Mesh.MeshId);
    }

    [Fact]
    public void Apply_TransformOnlyObject_DoesNotAddMeshRenderer()
    {
        var controller = CreateControllerWithTransformOnlyObject();
        var snapshot = EditorGuiSnapshotFactory.Create(controller);
        var inputState = new EditorInspectorInputState();
        inputState.SyncFrom(snapshot.Inspector);
        inputState.SetTextValues(snapshot.Inspector.ObjectId, "Edited Empty", "mesh://sphere", "material://highlight");
        inputState.SetTransformValues(new Vector3(4, 5, 6), Quaternion.Identity, Vector3.One);

        var result = inputState.Apply(controller, snapshot.Inspector);

        Assert.True(result, controller.LastError);
        Assert.True(controller.Session.IsDirty);
        Assert.Equal("Edited Empty", controller.Session.SelectedObject!.ObjectName);
        Assert.Equal(new Vector3(4, 5, 6), controller.Session.SelectedObject.LocalTransform.Position);
        Assert.Null(controller.Session.SelectedObject.MeshRendererComponent);
    }

    [Fact]
    public void Apply_DuplicateId_FailsAndKeepsSelectionAndDirty()
    {
        var controller = CreateControllerWithSelectedObject();
        var originalSelectedObjectId = controller.Session.SelectedObjectId!;
        var snapshot = EditorGuiSnapshotFactory.Create(controller);
        var inputState = new EditorInspectorInputState();
        inputState.SyncFrom(snapshot.Inspector);
        inputState.SetTextValues("cube-b", snapshot.Inspector.ObjectName, snapshot.Inspector.Mesh, snapshot.Inspector.Material);
        var initialDirty = controller.Session.IsDirty;

        var result = inputState.Apply(controller, snapshot.Inspector);

        Assert.False(result);
        Assert.Equal(initialDirty, controller.Session.IsDirty);
        Assert.Equal(originalSelectedObjectId, controller.Session.SelectedObjectId);
        Assert.Contains("duplicated", controller.LastError!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Apply_InvalidTransform_FailsAndRollsInputBackToSessionValue()
    {
        var controller = CreateControllerWithSelectedObject();
        var snapshot = EditorGuiSnapshotFactory.Create(controller);
        var inputState = new EditorInspectorInputState();
        inputState.SyncFrom(snapshot.Inspector);
        inputState.SetTransformValues(new Vector3(float.NaN, 0, 0), Quaternion.Identity, Vector3.One);
        var initialDirty = controller.Session.IsDirty;

        var result = inputState.Apply(controller, snapshot.Inspector);

        Assert.False(result);
        Assert.Equal(initialDirty, controller.Session.IsDirty);
        Assert.Equal(snapshot.Inspector.Position, inputState.Position);
        Assert.Contains("non-finite", controller.LastError!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Apply_ScriptRigidBodyAndBoxCollider_UpdatesSessionThroughController()
    {
        var controller = CreateControllerWithSelectedObject();
        var snapshot = EditorGuiSnapshotFactory.Create(controller);
        var inputState = new EditorInspectorInputState();
        inputState.SyncFrom(snapshot.Inspector);
        inputState.SetScriptValues("MoveOnInput", """{"speed": 2.25, "enabled": true, "label": "mover"}""");
        inputState.SetRigidBodyValues("Dynamic", 1.5d);
        inputState.SetBoxColliderValues(new Vector3(2.0f, 3.0f, 4.0f), new Vector3(0.5f, 0.0f, -0.5f));

        var result = inputState.Apply(controller, snapshot.Inspector);

        Assert.True(result, controller.LastError);
        var selected = controller.Session.SelectedObject!;
        var script = Assert.Single(selected.ScriptComponents);
        Assert.Equal("MoveOnInput", script.ScriptId);
        Assert.Equal(2.25d, script.Properties["speed"].Number);
        Assert.True(script.Properties["enabled"].Boolean);
        Assert.Equal("mover", script.Properties["label"].Text);
        Assert.Equal(SceneRigidBodyType.Dynamic, selected.RigidBodyComponent!.BodyType);
        Assert.Equal(1.5d, selected.RigidBodyComponent.Mass);
        Assert.Equal(new Vector3(2.0f, 3.0f, 4.0f), selected.BoxColliderComponent!.Size);
        Assert.Equal(new Vector3(0.5f, 0.0f, -0.5f), selected.BoxColliderComponent.Center);
    }

    [Fact]
    public void Apply_InvalidScriptPropertiesJson_FailsAndLeavesDocumentUnchanged()
    {
        var controller = CreateControllerWithSelectedObject();
        var snapshot = EditorGuiSnapshotFactory.Create(controller);
        var inputState = new EditorInspectorInputState();
        inputState.SyncFrom(snapshot.Inspector);
        inputState.SetScriptValues("MoveOnInput", "[1, 2]");
        var originalDocument = controller.Session.Document;
        var initialDirty = controller.Session.IsDirty;

        var result = inputState.Apply(controller, snapshot.Inspector);

        Assert.False(result);
        Assert.Same(originalDocument, controller.Session.Document);
        Assert.Equal(initialDirty, controller.Session.IsDirty);
        Assert.Empty(controller.Session.SelectedObject!.ScriptComponents);
        Assert.Contains("JSON object", controller.LastError!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddRemoveComponentHelpers_CallControllerSessionPaths()
    {
        var controller = CreateControllerWithSelectedObject();
        var snapshot = EditorGuiSnapshotFactory.Create(controller);
        var inputState = new EditorInspectorInputState();

        Assert.True(inputState.AddScriptComponent(controller, snapshot.Inspector), controller.LastError);
        snapshot = EditorGuiSnapshotFactory.Create(controller);
        Assert.Single(snapshot.Inspector.Scripts.Scripts);
        Assert.True(inputState.RemoveFirstScriptComponent(controller, snapshot.Inspector), controller.LastError);
        snapshot = EditorGuiSnapshotFactory.Create(controller);
        Assert.Empty(snapshot.Inspector.Scripts.Scripts);

        Assert.True(inputState.AddRigidBodyComponent(controller, snapshot.Inspector), controller.LastError);
        Assert.True(inputState.AddBoxColliderComponent(controller, EditorGuiSnapshotFactory.Create(controller).Inspector), controller.LastError);
        snapshot = EditorGuiSnapshotFactory.Create(controller);
        Assert.True(snapshot.Inspector.PhysicsParticipation.IsPhysicsReady);

        Assert.True(inputState.RemoveRigidBodyComponent(controller, snapshot.Inspector), controller.LastError);
        Assert.True(inputState.RemoveBoxColliderComponent(controller, EditorGuiSnapshotFactory.Create(controller).Inspector), controller.LastError);
        snapshot = EditorGuiSnapshotFactory.Create(controller);
        Assert.False(snapshot.Inspector.PhysicsParticipation.IsPhysicsReady);
    }

    private static EditorAppController CreateControllerWithSelectedObject()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenScene(WriteTemporarySceneFile()), controller.LastError);
        var objectId = controller.Session.Objects[0].ObjectId;
        Assert.True(controller.SelectObject(objectId), controller.LastError);
        if (controller.Session.Objects.Count == 1)
        {
            Assert.True(controller.AddObject(new Engine.SceneData.SceneFileObjectDefinition(
                "cube-b",
                "Cube B",
                new SceneFileComponentDefinition[]
                {
                    new SceneFileTransformComponentDefinition(new SceneFileTransformDefinition(null, null, null)),
                    new SceneFileMeshRendererComponentDefinition("mesh://cube", "material://default")
                })), controller.LastError);
            Assert.True(controller.SelectObject(objectId), controller.LastError);
        }

        return controller;
    }

    private static EditorAppController CreateControllerWithTransformOnlyObject()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenScene(WriteTransformOnlyTemporarySceneFile()), controller.LastError);
        Assert.True(controller.SelectObject("empty-main"), controller.LastError);
        return controller;
    }

    private static string WriteTemporarySceneFile()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), "AnsEngine.Editor.App.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        var scenePath = Path.Combine(directoryPath, "sample.scene.json");
        File.WriteAllText(
            scenePath,
            """
            {
              "version": "2.0",
              "scene": {
                "id": "editor-app-test-scene",
                "name": "Editor App Test Scene",
                "objects": [
                  {
                    "id": "cube-main",
                    "name": "Cube Main",
                    "components": [
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "MeshRenderer",
                        "mesh": "mesh://cube",
                        "material": "material://highlight"
                      }
                    ]
                  }
                ]
              }
            }
            """);
        return scenePath;
    }

    private static string WriteTransformOnlyTemporarySceneFile()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), "AnsEngine.Editor.App.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        var scenePath = Path.Combine(directoryPath, "transform-only.scene.json");
        File.WriteAllText(
            scenePath,
            """
            {
              "version": "2.0",
              "scene": {
                "id": "editor-app-test-scene",
                "name": "Editor App Test Scene",
                "objects": [
                  {
                    "id": "empty-main",
                    "name": "Empty Main",
                    "components": [
                      {
                        "type": "Transform"
                      }
                    ]
                  }
                ]
              }
            }
            """);
        return scenePath;
    }
}
