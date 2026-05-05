using Engine.Editor.App;
using System.Numerics;
using Xunit;

namespace Engine.Editor.App.Tests;

public sealed class EditorGuiSnapshotFactoryTests
{
    [Fact]
    public void Create_EmptySession_ReturnsStableLayoutAndStatus()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());

        var snapshot = EditorGuiSnapshotFactory.Create(controller);

        Assert.Equal(new[] { "Open", "Save", "Save As", "Add Object", "Remove Selected" }, snapshot.ToolbarLabels);
        Assert.Empty(snapshot.HierarchyItems);
        Assert.False(snapshot.Inspector.HasSelectedObject);
        Assert.Equal("No object selected.", snapshot.Inspector.EmptyMessage);
        Assert.Equal("<no scene>", snapshot.StatusBar.ScenePath);
        Assert.Equal("clean", snapshot.StatusBar.DirtyText);
        Assert.Equal("<none>", snapshot.StatusBar.SelectedObjectId);
        Assert.Equal(Vector2.Zero, snapshot.Layout.ToolbarPosition);
        Assert.Equal(0.0f, snapshot.Layout.HierarchyPosition.X);
        Assert.Equal(snapshot.Layout.ToolbarSize.Y, snapshot.Layout.HierarchyPosition.Y);
        Assert.Equal(snapshot.Layout.ToolbarSize.Y, snapshot.Layout.InspectorPosition.Y);
        Assert.Equal(snapshot.Layout.DisplaySize.Y - snapshot.Layout.StatusBarSize.Y, snapshot.Layout.StatusBarPosition.Y);
        Assert.Equal(snapshot.Layout.DisplaySize.X, snapshot.Layout.ToolbarSize.X);
        Assert.Equal(snapshot.Layout.DisplaySize.X, snapshot.Layout.StatusBarSize.X);
        Assert.Equal(new Vector2(8.0f, 5.0f), snapshot.Theme.FramePadding);
        Assert.Equal(4.0f, snapshot.Theme.WindowRounding);
    }

    [Fact]
    public void Create_CustomDisplaySize_ComputesStableDockedPanelLayout()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        var displaySize = new Vector2(1280.0f, 800.0f);

        var snapshot = EditorGuiSnapshotFactory.Create(controller, displaySize);

        Assert.Equal(displaySize, snapshot.Layout.DisplaySize);
        Assert.Equal(Vector2.Zero, snapshot.Layout.ToolbarPosition);
        Assert.Equal(new Vector2(displaySize.X, 56.0f), snapshot.Layout.ToolbarSize);
        Assert.Equal(new Vector2(0.0f, 56.0f), snapshot.Layout.HierarchyPosition);
        Assert.Equal(new Vector2(260.0f, 710.0f), snapshot.Layout.HierarchySize);
        Assert.Equal(new Vector2(920.0f, 56.0f), snapshot.Layout.InspectorPosition);
        Assert.Equal(new Vector2(360.0f, 710.0f), snapshot.Layout.InspectorSize);
        Assert.Equal(new Vector2(260.0f, 56.0f), snapshot.Layout.SceneViewPosition);
        Assert.Equal(new Vector2(660.0f, 710.0f), snapshot.Layout.SceneViewSize);
        Assert.Equal(new Vector2(0.0f, 766.0f), snapshot.Layout.StatusBarPosition);
        Assert.Equal(new Vector2(displaySize.X, 34.0f), snapshot.Layout.StatusBarSize);
    }

    [Fact]
    public void Create_OpenSession_UsesSessionStateForStatus()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenStartupScene(), controller.LastError);

        var snapshot = EditorGuiSnapshotFactory.Create(controller);

        Assert.NotEmpty(snapshot.HierarchyItems);
        Assert.True(snapshot.ScenePreview.IsNonBlank);
        Assert.EndsWith(Path.Combine("src", "Engine.App", "SampleScenes", "default.scene.json"), snapshot.StatusBar.ScenePath);
        Assert.Equal("clean", snapshot.StatusBar.DirtyText);
        Assert.Equal("<none>", snapshot.StatusBar.SelectedObjectId);
    }

    [Fact]
    public void Create_LastError_PropagatesToStatusBar()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        controller.SetLastError("not ready");

        var snapshot = EditorGuiSnapshotFactory.Create(controller);

        Assert.Equal("not ready", snapshot.StatusBar.LastError);
    }

    [Fact]
    public void Create_SelectedObject_PropagatesSelectionToHierarchyInspectorAndStatus()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenStartupScene(), controller.LastError);
        var objectId = controller.Session.Objects[0].ObjectId;

        Assert.True(controller.SelectObject(objectId), controller.LastError);
        var snapshot = EditorGuiSnapshotFactory.Create(controller);

        Assert.False(controller.Session.IsDirty);
        Assert.Equal(objectId, snapshot.StatusBar.SelectedObjectId);
        Assert.True(snapshot.Inspector.HasSelectedObject);
        Assert.Equal(objectId, snapshot.Inspector.ObjectId);
        Assert.Equal("Object", snapshot.Inspector.Object.Title);
        Assert.Equal("Transform", snapshot.Inspector.Transform.Title);
        Assert.Equal("MeshRenderer", snapshot.Inspector.MeshRenderer.Title);
        Assert.Equal("Scripts", snapshot.Inspector.Scripts.Title);
        Assert.Equal("RigidBody", snapshot.Inspector.RigidBody.Title);
        Assert.Equal("BoxCollider", snapshot.Inspector.BoxCollider.Title);
        Assert.Equal("PhysicsParticipation", snapshot.Inspector.PhysicsParticipation.Title);
        Assert.True(snapshot.Inspector.Transform.HasTransform);
        Assert.True(snapshot.Inspector.MeshRenderer.HasMeshRenderer);
        Assert.Equal(
            snapshot.Inspector.Transform.HasTransform &&
            snapshot.Inspector.RigidBody.HasRigidBody &&
            snapshot.Inspector.BoxCollider.HasBoxCollider,
            snapshot.Inspector.PhysicsParticipation.IsPhysicsReady);
        Assert.Contains(snapshot.HierarchyItems, item => item.ObjectId == objectId && item.IsSelected);
    }

    [Fact]
    public void Create_SelectedAuthoringObject_ShowsScriptPhysicsGroupsAndReadyState()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenScene(WriteAuthoringTemporarySceneFile()), controller.LastError);
        Assert.True(controller.SelectObject("mover"), controller.LastError);

        var snapshot = EditorGuiSnapshotFactory.Create(controller);

        var script = Assert.Single(snapshot.Inspector.Scripts.Scripts);
        Assert.Equal("MoveOnInput", script.ScriptId);
        Assert.Contains("\"speed\":2", script.PropertiesJson);
        Assert.True(snapshot.Inspector.RigidBody.HasRigidBody);
        Assert.Equal("Dynamic", snapshot.Inspector.RigidBody.BodyType);
        Assert.Equal(1.0d, snapshot.Inspector.RigidBody.Mass);
        Assert.True(snapshot.Inspector.BoxCollider.HasBoxCollider);
        Assert.Equal(Vector3.One, snapshot.Inspector.BoxCollider.Size);
        Assert.True(snapshot.Inspector.PhysicsParticipation.IsPhysicsReady);
    }

    [Fact]
    public void Create_SelectedTransformOnlyObject_ShowsMeshRendererEmptyState()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenScene(WriteTransformOnlyTemporarySceneFile()), controller.LastError);

        Assert.True(controller.SelectObject("empty-main"), controller.LastError);
        var snapshot = EditorGuiSnapshotFactory.Create(controller);

        Assert.True(snapshot.Inspector.HasSelectedObject);
        Assert.Equal("empty-main", snapshot.Inspector.ObjectId);
        Assert.True(snapshot.Inspector.Transform.HasTransform);
        Assert.False(snapshot.Inspector.MeshRenderer.HasMeshRenderer);
        Assert.Equal("No MeshRenderer component.", snapshot.Inspector.MeshRenderer.EmptyMessage);
    }

    [Fact]
    public void Create_SelectMissingObject_KeepsExistingSelectionAndReportsLastError()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenStartupScene(), controller.LastError);
        var objectId = controller.Session.Objects[0].ObjectId;
        Assert.True(controller.SelectObject(objectId), controller.LastError);

        Assert.False(controller.SelectObject("missing-object"));
        var snapshot = EditorGuiSnapshotFactory.Create(controller);

        Assert.False(controller.Session.IsDirty);
        Assert.Equal(objectId, snapshot.StatusBar.SelectedObjectId);
        Assert.Equal(objectId, snapshot.Inspector.ObjectId);
        Assert.Contains("not found", snapshot.StatusBar.LastError, StringComparison.OrdinalIgnoreCase);
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

    private static string WriteAuthoringTemporarySceneFile()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), "AnsEngine.Editor.App.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        var scenePath = Path.Combine(directoryPath, "authoring.scene.json");
        File.WriteAllText(
            scenePath,
            """
            {
              "version": "2.0",
              "scene": {
                "id": "editor-app-authoring-scene",
                "name": "Editor App Authoring Scene",
                "objects": [
                  {
                    "id": "mover",
                    "name": "Mover",
                    "components": [
                      {
                        "type": "Transform"
                      },
                      {
                        "type": "Script",
                        "scriptId": "MoveOnInput",
                        "properties": {
                          "speed": 2
                        }
                      },
                      {
                        "type": "RigidBody",
                        "bodyType": "Dynamic",
                        "mass": 1
                      },
                      {
                        "type": "BoxCollider",
                        "size": {
                          "x": 1,
                          "y": 1,
                          "z": 1
                        }
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
