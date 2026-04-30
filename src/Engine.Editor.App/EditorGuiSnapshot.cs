namespace Engine.Editor.App;

public sealed record EditorGuiSnapshot(
    IReadOnlyList<string> ToolbarLabels,
    IReadOnlyList<EditorHierarchyItemSnapshot> HierarchyItems,
    EditorInspectorSnapshot Inspector,
    EditorStatusBarSnapshot StatusBar);
