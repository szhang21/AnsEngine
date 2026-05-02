using System.Numerics;
using Engine.Editor.App;
using Xunit;

namespace Engine.Editor.App.Tests;

public sealed class EditorFileWorkflowStateTests
{
    [Fact]
    public void ResolveStartupScenePath_EnvironmentOverride_WinsOverDefault()
    {
        var originalValue = Environment.GetEnvironmentVariable("ANS_ENGINE_EDITOR_SCENE_PATH");
        var overridePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "override.scene.json");
        try
        {
            Environment.SetEnvironmentVariable("ANS_ENGINE_EDITOR_SCENE_PATH", overridePath);

            var resolvedPath = new EditorScenePathResolver().ResolveStartupScenePath();

            Assert.Equal(Path.GetFullPath(overridePath), resolvedPath);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ANS_ENGINE_EDITOR_SCENE_PATH", originalValue);
        }
    }

    [Fact]
    public void Save_ModifiedScene_WritesFileAndClearsDirty()
    {
        var scenePath = CopySampleSceneToTemporaryFile();
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenScene(scenePath), controller.LastError);
        var objectId = controller.Session.Objects[0].ObjectId;
        Assert.True(controller.SelectObject(objectId), controller.LastError);
        Assert.True(controller.UpdateObjectName(objectId, "Saved Name"), controller.LastError);
        Assert.True(controller.Session.IsDirty);

        var workflow = new EditorFileWorkflowState();
        workflow.SyncFrom(EditorGuiSnapshotFactory.Create(controller));
        var result = workflow.Save(controller);

        Assert.True(result, controller.LastError);
        Assert.False(controller.Session.IsDirty);
        Assert.Contains("Saved Name", File.ReadAllText(scenePath));
    }

    [Fact]
    public void SaveAs_Success_UpdatesSessionPathAndClearsDirty()
    {
        var scenePath = CopySampleSceneToTemporaryFile();
        var saveAsPath = Path.Combine(Path.GetDirectoryName(scenePath)!, "saved-as.scene.json");
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenScene(scenePath), controller.LastError);
        var objectId = controller.Session.Objects[0].ObjectId;
        Assert.True(controller.UpdateObjectName(objectId, "Saved As Name"), controller.LastError);
        var workflow = new EditorFileWorkflowState();
        workflow.SyncFrom(EditorGuiSnapshotFactory.Create(controller));
        workflow.SetSaveAsPath(saveAsPath);

        var result = workflow.SaveAs(controller);

        Assert.True(result, controller.LastError);
        Assert.False(controller.Session.IsDirty);
        Assert.Equal(saveAsPath, controller.Session.SceneFilePath);
        Assert.Contains("Saved As Name", File.ReadAllText(saveAsPath));
    }

    [Fact]
    public void Open_MissingFile_KeepsCurrentSession()
    {
        var scenePath = CopySampleSceneToTemporaryFile();
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenScene(scenePath), controller.LastError);
        var originalSceneId = controller.Session.Scene!.SceneId;
        var workflow = new EditorFileWorkflowState();
        workflow.SyncFrom(EditorGuiSnapshotFactory.Create(controller));
        workflow.SetOpenPath(Path.Combine(Path.GetDirectoryName(scenePath)!, "missing.scene.json"));

        var result = workflow.Open(controller);

        Assert.False(result);
        Assert.True(controller.Session.HasDocument);
        Assert.Equal(scenePath, controller.Session.SceneFilePath);
        Assert.Equal(originalSceneId, controller.Session.Scene!.SceneId);
    }

    [Fact]
    public void SaveAs_InvalidPath_KeepsDirtyAndCurrentPath()
    {
        var scenePath = CopySampleSceneToTemporaryFile();
        var controller = new EditorAppController(new EditorScenePathResolver());
        Assert.True(controller.OpenScene(scenePath), controller.LastError);
        var objectId = controller.Session.Objects[0].ObjectId;
        Assert.True(controller.UpdateObjectTransformComponent(
            objectId,
            new Engine.SceneData.SceneFileTransformDefinition(new Vector3(4, 5, 6), Quaternion.Identity, Vector3.One)), controller.LastError);
        var workflow = new EditorFileWorkflowState();
        workflow.SyncFrom(EditorGuiSnapshotFactory.Create(controller));
        workflow.SetSaveAsPath(string.Empty);

        var result = workflow.SaveAs(controller);

        Assert.False(result);
        Assert.True(controller.Session.IsDirty);
        Assert.Equal(scenePath, controller.Session.SceneFilePath);
    }

    private static string CopySampleSceneToTemporaryFile()
    {
        var sourcePath = new EditorScenePathResolver().ResolveStartupScenePath();
        var directoryPath = Path.Combine(Path.GetTempPath(), "AnsEngine.Editor.App.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        var destinationPath = Path.Combine(directoryPath, "sample.scene.json");
        File.Copy(sourcePath, destinationPath);
        return destinationPath;
    }
}
