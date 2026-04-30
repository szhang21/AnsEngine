namespace Engine.Editor.App;

public sealed class EditorFileWorkflowState
{
    private string? mSyncedScenePath;

    public string OpenPath { get; private set; } = string.Empty;

    public string SaveAsPath { get; private set; } = string.Empty;

    public void SyncFrom(EditorGuiSnapshot snapshot)
    {
        var scenePath = snapshot.StatusBar.ScenePath == "<no scene>"
            ? string.Empty
            : snapshot.StatusBar.ScenePath;
        if (string.Equals(mSyncedScenePath, scenePath, StringComparison.Ordinal))
        {
            return;
        }

        mSyncedScenePath = scenePath;
        OpenPath = scenePath;
        SaveAsPath = scenePath;
    }

    public void SetOpenPath(string openPath)
    {
        OpenPath = openPath;
    }

    public void SetSaveAsPath(string saveAsPath)
    {
        SaveAsPath = saveAsPath;
    }

    public bool Open(EditorAppController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        var result = controller.OpenScene(OpenPath);
        ResetFromController(controller);
        return result;
    }

    public bool Save(EditorAppController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        var result = controller.Save();
        ResetFromController(controller);
        return result;
    }

    public bool SaveAs(EditorAppController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        var result = controller.SaveAs(SaveAsPath);
        ResetFromController(controller);
        return result;
    }

    private void ResetFromController(EditorAppController controller)
    {
        mSyncedScenePath = null;
        SyncFrom(EditorGuiSnapshotFactory.Create(controller));
    }
}
