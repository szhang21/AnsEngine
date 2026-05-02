using Engine.Editor.App;
using Xunit;

namespace Engine.Editor.App.Tests;

public sealed class EditorObjectWorkflowStateTests
{
    [Fact]
    public void AddObject_CreatesDefaultCubeAndSelectsIt()
    {
        var controller = CreateController();
        var workflow = new EditorObjectWorkflowState();

        var result = workflow.AddObject(controller);

        Assert.True(result, controller.LastError);
        Assert.True(controller.Session.IsDirty);
        Assert.Equal("object-001", controller.Session.SelectedObjectId);
        var selectedObject = controller.Session.SelectedObject!;
        Assert.Equal("mesh://cube", selectedObject.MeshRendererComponent!.Mesh.MeshId);
        Assert.Equal("material://default", selectedObject.MeshRendererComponent.Material.MaterialId);
        Assert.Equal(System.Numerics.Vector3.Zero, selectedObject.TransformComponent!.Transform.Position);
        Assert.Equal(System.Numerics.Vector3.One, selectedObject.TransformComponent.Transform.Scale);
    }

    [Fact]
    public void AddObject_FillsSmallestAvailableObjectIdGap()
    {
        var factory = new EditorDefaultObjectFactory();
        var existingObjects = new[]
        {
            new EditorHierarchyItemSnapshot("object-001", "object-001", false),
            new EditorHierarchyItemSnapshot("object-003", "object-003", false)
        };

        var objectDefinition = factory.Create(existingObjects);

        Assert.Equal("object-002", objectDefinition.Id);
    }

    [Fact]
    public void RemoveSelectedObject_SelectedObject_RemovesAndClearsSelection()
    {
        var controller = CreateController();
        var workflow = new EditorObjectWorkflowState();
        Assert.True(workflow.AddObject(controller), controller.LastError);

        var result = workflow.RemoveSelectedObject(controller);

        Assert.True(result, controller.LastError);
        Assert.True(controller.Session.IsDirty);
        Assert.Null(controller.Session.SelectedObjectId);
        Assert.DoesNotContain(controller.Session.Objects, item => item.ObjectId == "object-001");
    }

    [Fact]
    public void RemoveSelectedObject_NoSelection_ReturnsLastError()
    {
        var controller = CreateController();
        var workflow = new EditorObjectWorkflowState();

        var result = workflow.RemoveSelectedObject(controller);

        Assert.False(result);
        Assert.False(controller.Session.IsDirty);
        Assert.Contains("No scene object is selected", controller.LastError);
    }

    [Fact]
    public void AddRemoveSaveReload_PersistsObjectChanges()
    {
        var scenePath = CopySampleSceneToTemporaryFile();
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenScene(scenePath), controller.LastError);
        var workflow = new EditorObjectWorkflowState();
        Assert.True(workflow.AddObject(controller), controller.LastError);
        Assert.True(controller.Save(), controller.LastError);

        var reloadController = new EditorAppController(new EditorScenePathResolver());
        Assert.True(reloadController.OpenScene(scenePath), reloadController.LastError);
        Assert.Contains(reloadController.Session.Objects, item => item.ObjectId == "object-001");

        Assert.True(reloadController.SelectObject("object-001"), reloadController.LastError);
        Assert.True(new EditorObjectWorkflowState().RemoveSelectedObject(reloadController), reloadController.LastError);
        Assert.True(reloadController.Save(), reloadController.LastError);

        var finalController = new EditorAppController(new EditorScenePathResolver());
        Assert.True(finalController.OpenScene(scenePath), finalController.LastError);
        Assert.DoesNotContain(finalController.Session.Objects, item => item.ObjectId == "object-001");
    }

    private static EditorAppController CreateController()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenScene(WriteTemporarySceneFile()), controller.LastError);
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

    private static string CopySampleSceneToTemporaryFile()
    {
        var sourcePath = WriteTemporarySceneFile();
        var directoryPath = Path.Combine(Path.GetTempPath(), "AnsEngine.Editor.App.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        var destinationPath = Path.Combine(directoryPath, "sample.scene.json");
        File.Copy(sourcePath, destinationPath);
        return destinationPath;
    }
}
