using System.Numerics;
using Engine.Editor.App;
using Xunit;

namespace Engine.Editor.App.Tests;

public sealed class EditorInspectorInputStateTests
{
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

    private static EditorAppController CreateControllerWithSelectedObject()
    {
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenStartupScene(), controller.LastError);
        var objectId = controller.Session.Objects[0].ObjectId;
        Assert.True(controller.SelectObject(objectId), controller.LastError);
        if (controller.Session.Objects.Count == 1)
        {
            Assert.True(controller.AddObject(new Engine.SceneData.SceneFileObjectDefinition(
                "cube-b",
                "Cube B",
                "mesh://cube",
                "material://default",
                null)), controller.LastError);
            Assert.True(controller.SelectObject(objectId), controller.LastError);
        }

        return controller;
    }
}
