namespace Engine.Editor.App;

public sealed record EditorStatusBarSnapshot(
    string ScenePath,
    bool IsDirty,
    string DirtyText,
    string SelectedObjectId,
    string LastError);
