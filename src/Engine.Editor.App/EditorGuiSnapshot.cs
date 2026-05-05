using System.Numerics;

namespace Engine.Editor.App;

public sealed record EditorGuiSnapshot(
    IReadOnlyList<string> ToolbarLabels,
    IReadOnlyList<EditorHierarchyItemSnapshot> HierarchyItems,
    EditorInspectorSnapshot Inspector,
    EditorStatusBarSnapshot StatusBar,
    EditorScenePreviewSnapshot ScenePreview,
    EditorGuiLayoutSnapshot Layout,
    EditorGuiThemeSnapshot Theme);

public sealed record EditorGuiLayoutSnapshot(
    Vector2 DisplaySize,
    Vector2 ToolbarPosition,
    Vector2 ToolbarSize,
    Vector2 HierarchyPosition,
    Vector2 HierarchySize,
    Vector2 SceneViewPosition,
    Vector2 SceneViewSize,
    Vector2 InspectorPosition,
    Vector2 InspectorSize,
    Vector2 StatusBarPosition,
    Vector2 StatusBarSize);

public sealed record EditorGuiThemeSnapshot(
    Vector4 WindowBackground,
    Vector4 PanelBackground,
    Vector4 Header,
    Vector4 HeaderHovered,
    Vector4 HeaderActive,
    Vector4 Border,
    Vector2 FramePadding,
    Vector2 ItemSpacing,
    float WindowRounding,
    float FrameRounding,
    float ChildRounding);
