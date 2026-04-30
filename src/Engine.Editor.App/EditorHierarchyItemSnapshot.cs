namespace Engine.Editor.App;

public sealed record EditorHierarchyItemSnapshot(
    string ObjectId,
    string DisplayName,
    bool IsSelected);
