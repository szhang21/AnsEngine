using Engine.Editor.App;
using Engine.SceneData;
using System.Numerics;
using Xunit;

namespace Engine.Editor.App.Tests;

public sealed class EditorAppControllerTests
{
    [Fact]
    public void OpenStartupScene_DefaultResolver_LoadsSourceSampleScene()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());

        var result = controller.OpenStartupScene();

        Assert.True(result, controller.LastError);
        Assert.True(controller.Session.HasDocument);
        Assert.False(controller.Session.IsDirty);
        Assert.EndsWith(Path.Combine("src", "Engine.App", "SampleScenes", "default.scene.json"), controller.Session.SceneFilePath);
    }

    [Fact]
    public void OpenScene_MissingPath_StoresLastErrorAndKeepsEmptySession()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.scene.json");

        var result = controller.OpenScene(missingPath);

        Assert.False(result);
        Assert.False(controller.Session.HasDocument);
        Assert.False(string.IsNullOrWhiteSpace(controller.LastError));
    }

    [Fact]
    public void OpenScene_LoadsNonBlankEditTimePreview()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());

        var result = controller.OpenStartupScene();

        Assert.True(result, controller.LastError);
        Assert.True(controller.PreviewSnapshot.HasScene);
        Assert.True(controller.PreviewSnapshot.IsNonBlank);
        Assert.True(controller.PreviewSnapshot.RenderItemCount > 0);
        Assert.True(controller.PreviewSnapshot.BatchCount > 0);
        Assert.True(controller.PreviewSnapshot.MeshVertexCount > 0);
    }

    [Fact]
    public void ApplyAndSave_RefreshPreviewWithoutRuntimeApp()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenScene(WriteTemporarySceneFile()), controller.LastError);
        Assert.True(controller.SelectObject("cube-main"), controller.LastError);
        var openedVersion = controller.PreviewSnapshot.RefreshVersion;

        Assert.True(
            controller.UpdateObjectTransformComponent(
                "cube-main",
                new SceneFileTransformDefinition(new Vector3(2.0f, 0.0f, 0.0f), null, null)),
            controller.LastError);
        var editedVersion = controller.PreviewSnapshot.RefreshVersion;
        Assert.True(controller.Save(), controller.LastError);

        Assert.True(editedVersion > openedVersion);
        Assert.True(controller.PreviewSnapshot.RefreshVersion > editedVersion);
        Assert.True(controller.PreviewSnapshot.IsNonBlank);
    }

    private static string WriteTemporarySceneFile()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), "AnsEngine.Editor.App.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        var scenePath = Path.Combine(directoryPath, "preview.scene.json");
        File.WriteAllText(
            scenePath,
            """
            {
              "version": "2.0",
              "scene": {
                "id": "editor-preview-scene",
                "name": "Editor Preview Scene",
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
                        "material": "material://default"
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
