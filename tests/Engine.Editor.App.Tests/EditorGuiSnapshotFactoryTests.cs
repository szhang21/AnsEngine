using Engine.Editor.App;
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
    }

    [Fact]
    public void Create_OpenSession_UsesSessionStateForStatus()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenStartupScene(), controller.LastError);

        var snapshot = EditorGuiSnapshotFactory.Create(controller);

        Assert.NotEmpty(snapshot.HierarchyItems);
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
        Assert.Contains(snapshot.HierarchyItems, item => item.ObjectId == objectId && item.IsSelected);
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
}
