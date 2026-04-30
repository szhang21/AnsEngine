using System.Numerics;

namespace Engine.Editor.App;

public sealed record EditorGuiSnapshot(
    IReadOnlyList<string> ToolbarLabels,
    IReadOnlyList<EditorHierarchyItemSnapshot> HierarchyItems,
    EditorInspectorSnapshot Inspector,
    EditorStatusBarSnapshot StatusBar,
    EditorGuiLayoutSnapshot Layout);

public sealed record EditorGuiLayoutSnapshot(
    Vector2 DisplaySize,
    Vector2 ToolbarPosition,
    Vector2 ToolbarSize,
    Vector2 HierarchyPosition,
    Vector2 HierarchySize,
    Vector2 MainWorkspacePosition,
    Vector2 MainWorkspaceSize,
    Vector2 InspectorPosition,
    Vector2 InspectorSize,
    Vector2 StatusBarPosition,
    Vector2 StatusBarSize);
