using System.Numerics;

namespace Engine.Editor.App;

public static class EditorGuiSnapshotFactory
{
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

        return new EditorGuiSnapshot(sToolbarLabels, hierarchyItems, inspector, statusBar);
    }
}
