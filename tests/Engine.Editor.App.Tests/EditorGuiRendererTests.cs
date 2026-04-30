using Engine.Editor.App;
using Xunit;

namespace Engine.Editor.App.Tests;

public sealed class EditorGuiRendererTests
{
    [Fact]
    public void RenderFrame_WhenNativeImGuiFramesDisabled_CapturesSnapshotWithoutNativeFrame()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        var renderer = new EditorGuiRenderer(controller);

        renderer.RenderFrame(false);

        var snapshot = Assert.IsType<EditorGuiSnapshot>(renderer.LastSnapshot);
        Assert.Equal("<no scene>", snapshot.StatusBar.ScenePath);
    }
}
