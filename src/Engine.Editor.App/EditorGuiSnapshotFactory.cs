using System.Numerics;

namespace Engine.Editor.App;

public static class EditorGuiSnapshotFactory
{
    private static readonly Vector2 sDefaultDisplaySize = new(1100.0f, 720.0f);
    private const float kToolbarHeight = 104.0f;
    private const float kStatusBarHeight = 34.0f;
    private const float kDefaultHierarchyWidth = 260.0f;
    private const float kDefaultInspectorWidth = 360.0f;
    private const float kMinimumPanelWidth = 160.0f;
    private const float kMinimumMainWorkspaceWidth = 160.0f;

    private static readonly string[] sToolbarLabels =
    {
        "Open",
        "Save",
        "Save As",
        "Add Object",
        "Remove Selected"
    };

    public static EditorGuiSnapshot Create(EditorAppController controller)
    {
        return Create(controller, sDefaultDisplaySize);
    }

    public static EditorGuiSnapshot Create(EditorAppController controller, Vector2 displaySize)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var hierarchyItems = controller.Session.Objects
            .Select(item => new EditorHierarchyItemSnapshot(
                item.ObjectId,
                string.IsNullOrWhiteSpace(item.ObjectName) ? item.ObjectId : $"{item.ObjectName} ({item.ObjectId})",
                string.Equals(item.ObjectId, controller.Session.SelectedObjectId, StringComparison.Ordinal)))
            .ToArray();

        var selectedObject = controller.Session.SelectedObject;
        var inspector = selectedObject is null
            ? new EditorInspectorSnapshot(
                false,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                Vector3.Zero,
                Quaternion.Identity,
                Vector3.One,
                "No object selected.")
            : new EditorInspectorSnapshot(
                true,
                selectedObject.ObjectId,
                selectedObject.ObjectName,
                selectedObject.Mesh.MeshId,
                selectedObject.Material.MaterialId,
                selectedObject.LocalTransform.Position,
                selectedObject.LocalTransform.Rotation,
                selectedObject.LocalTransform.Scale,
                string.Empty);

        var scenePath = string.IsNullOrWhiteSpace(controller.Session.SceneFilePath)
            ? "<no scene>"
            : controller.Session.SceneFilePath!;
        var selectedObjectId = string.IsNullOrWhiteSpace(controller.Session.SelectedObjectId)
            ? "<none>"
            : controller.Session.SelectedObjectId!;
        var statusBar = new EditorStatusBarSnapshot(
            scenePath,
            controller.Session.IsDirty,
            controller.Session.IsDirty ? "dirty" : "clean",
            selectedObjectId,
            controller.LastError ?? string.Empty);

        return new EditorGuiSnapshot(sToolbarLabels, hierarchyItems, inspector, statusBar, CreateLayout(displaySize));
    }

    private static EditorGuiLayoutSnapshot CreateLayout(Vector2 displaySize)
    {
        var width = Math.Max(1.0f, displaySize.X);
        var height = Math.Max(kToolbarHeight + kStatusBarHeight + 1.0f, displaySize.Y);
        var contentHeight = Math.Max(1.0f, height - kToolbarHeight - kStatusBarHeight);
        var hierarchyWidth = Math.Min(kDefaultHierarchyWidth, Math.Max(kMinimumPanelWidth, width * 0.22f));
        var inspectorWidth = Math.Min(kDefaultInspectorWidth, Math.Max(kMinimumPanelWidth, width * 0.32f));
        if (hierarchyWidth + inspectorWidth + kMinimumMainWorkspaceWidth > width)
        {
            var sideWidth = Math.Max(0.0f, width - kMinimumMainWorkspaceWidth);
            hierarchyWidth = sideWidth * 0.42f;
            inspectorWidth = sideWidth - hierarchyWidth;
        }

        var mainWorkspaceWidth = Math.Max(1.0f, width - hierarchyWidth - inspectorWidth);
        return new EditorGuiLayoutSnapshot(
            new Vector2(width, height),
            Vector2.Zero,
            new Vector2(width, kToolbarHeight),
            new Vector2(0.0f, kToolbarHeight),
            new Vector2(hierarchyWidth, contentHeight),
            new Vector2(hierarchyWidth, kToolbarHeight),
            new Vector2(mainWorkspaceWidth, contentHeight),
            new Vector2(hierarchyWidth + mainWorkspaceWidth, kToolbarHeight),
            new Vector2(inspectorWidth, contentHeight),
            new Vector2(0.0f, height - kStatusBarHeight),
            new Vector2(width, kStatusBarHeight));
    }
}
