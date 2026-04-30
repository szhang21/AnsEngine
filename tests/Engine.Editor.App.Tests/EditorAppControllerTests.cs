using Engine.Editor.App;
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
}
