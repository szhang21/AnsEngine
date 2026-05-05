using System.Numerics;
using System.Text.Json;
using Engine.SceneData;

namespace Engine.Editor.App;

public static class EditorGuiSnapshotFactory
{
    private static readonly Vector2 sDefaultDisplaySize = new(1100.0f, 720.0f);
    private static readonly EditorGuiThemeSnapshot sTheme = new(
        new Vector4(0.09f, 0.10f, 0.11f, 1.0f),
        new Vector4(0.13f, 0.14f, 0.15f, 1.0f),
        new Vector4(0.22f, 0.26f, 0.30f, 1.0f),
        new Vector4(0.30f, 0.36f, 0.42f, 1.0f),
        new Vector4(0.36f, 0.43f, 0.50f, 1.0f),
        new Vector4(0.24f, 0.26f, 0.28f, 1.0f),
        new Vector2(8.0f, 5.0f),
        new Vector2(8.0f, 6.0f),
        4.0f,
        3.0f,
        3.0f);
    private const float kToolbarHeight = 56.0f;
    private const float kStatusBarHeight = 34.0f;
    private const float kDefaultHierarchyWidth = 260.0f;
    private const float kDefaultInspectorWidth = 360.0f;
    private const float kMinimumPanelWidth = 160.0f;
    private const float kMinimumSceneViewWidth = 160.0f;

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
                new EditorInspectorObjectGroupSnapshot("Object", string.Empty, string.Empty),
                new EditorInspectorTransformGroupSnapshot("Transform", false, Vector3.Zero, Quaternion.Identity, Vector3.One),
                new EditorInspectorMeshRendererGroupSnapshot("MeshRenderer", false, string.Empty, string.Empty, string.Empty),
                new EditorInspectorScriptsGroupSnapshot("Scripts", Array.Empty<EditorInspectorScriptSnapshot>(), "No Script components."),
                new EditorInspectorRigidBodyGroupSnapshot("RigidBody", false, "Dynamic", 1.0d, "No RigidBody component."),
                new EditorInspectorBoxColliderGroupSnapshot("BoxCollider", false, Vector3.One, Vector3.Zero, "No BoxCollider component."),
                new EditorInspectorPhysicsParticipationSnapshot("PhysicsParticipation", false, false, false),
                "No object selected.")
            : new EditorInspectorSnapshot(
                true,
                new EditorInspectorObjectGroupSnapshot(
                    "Object",
                    selectedObject.ObjectId,
                    selectedObject.ObjectName),
                new EditorInspectorTransformGroupSnapshot(
                    "Transform",
                    selectedObject.TransformComponent is not null,
                    selectedObject.LocalTransform.Position,
                    selectedObject.LocalTransform.Rotation,
                    selectedObject.LocalTransform.Scale),
                new EditorInspectorMeshRendererGroupSnapshot(
                    "MeshRenderer",
                    selectedObject.MeshRendererComponent is not null,
                    selectedObject.MeshRendererComponent?.Mesh.MeshId ?? string.Empty,
                    selectedObject.MeshRendererComponent?.Material.MaterialId ?? string.Empty,
                    selectedObject.MeshRendererComponent is null ? "No MeshRenderer component." : string.Empty),
                new EditorInspectorScriptsGroupSnapshot(
                    "Scripts",
                    selectedObject.ScriptComponents
                        .Select(item => new EditorInspectorScriptSnapshot(item.ScriptId, SerializeScriptProperties(item.Properties)))
                        .ToArray(),
                    selectedObject.ScriptComponents.Count == 0 ? "No Script components." : string.Empty),
                new EditorInspectorRigidBodyGroupSnapshot(
                    "RigidBody",
                    selectedObject.RigidBodyComponent is not null,
                    selectedObject.RigidBodyComponent?.BodyType.ToString() ?? "Dynamic",
                    selectedObject.RigidBodyComponent?.Mass ?? 1.0d,
                    selectedObject.RigidBodyComponent is null ? "No RigidBody component." : string.Empty),
                new EditorInspectorBoxColliderGroupSnapshot(
                    "BoxCollider",
                    selectedObject.BoxColliderComponent is not null,
                    selectedObject.BoxColliderComponent?.Size ?? Vector3.One,
                    selectedObject.BoxColliderComponent?.Center ?? Vector3.Zero,
                    selectedObject.BoxColliderComponent is null ? "No BoxCollider component." : string.Empty),
                new EditorInspectorPhysicsParticipationSnapshot(
                    "PhysicsParticipation",
                    selectedObject.TransformComponent is not null,
                    selectedObject.RigidBodyComponent is not null,
                    selectedObject.BoxColliderComponent is not null),
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

        return new EditorGuiSnapshot(
            sToolbarLabels,
            hierarchyItems,
            inspector,
            statusBar,
            controller.PreviewSnapshot,
            CreateLayout(displaySize),
            sTheme);
    }

    private static EditorGuiLayoutSnapshot CreateLayout(Vector2 displaySize)
    {
        var width = Math.Max(1.0f, displaySize.X);
        var height = Math.Max(kToolbarHeight + kStatusBarHeight + 1.0f, displaySize.Y);
        var contentHeight = Math.Max(1.0f, height - kToolbarHeight - kStatusBarHeight);
        var hierarchyWidth = Math.Min(kDefaultHierarchyWidth, Math.Max(kMinimumPanelWidth, width * 0.22f));
        var inspectorWidth = Math.Min(kDefaultInspectorWidth, Math.Max(kMinimumPanelWidth, width * 0.32f));
        if (hierarchyWidth + inspectorWidth + kMinimumSceneViewWidth > width)
        {
            var sideWidth = Math.Max(0.0f, width - kMinimumSceneViewWidth);
            hierarchyWidth = sideWidth * 0.42f;
            inspectorWidth = sideWidth - hierarchyWidth;
        }

        var sceneViewWidth = Math.Max(1.0f, width - hierarchyWidth - inspectorWidth);
        return new EditorGuiLayoutSnapshot(
            new Vector2(width, height),
            Vector2.Zero,
            new Vector2(width, kToolbarHeight),
            new Vector2(0.0f, kToolbarHeight),
            new Vector2(hierarchyWidth, contentHeight),
            new Vector2(hierarchyWidth, kToolbarHeight),
            new Vector2(sceneViewWidth, contentHeight),
            new Vector2(hierarchyWidth + sceneViewWidth, kToolbarHeight),
            new Vector2(inspectorWidth, contentHeight),
            new Vector2(0.0f, height - kStatusBarHeight),
            new Vector2(width, kStatusBarHeight));
    }

    private static string SerializeScriptProperties(IReadOnlyDictionary<string, SceneScriptPropertyValue> properties)
    {
        var fileProperties = new Dictionary<string, SceneFileScriptPropertyValue>(StringComparer.Ordinal);
        foreach (var property in properties)
        {
            if (property.Value.IsNumber)
            {
                fileProperties[property.Key] = SceneFileScriptPropertyValue.FromNumber(property.Value.Number!.Value);
                continue;
            }

            if (property.Value.IsBoolean)
            {
                fileProperties[property.Key] = SceneFileScriptPropertyValue.FromBoolean(property.Value.Boolean!.Value);
                continue;
            }

            fileProperties[property.Key] = SceneFileScriptPropertyValue.FromString(property.Value.Text ?? string.Empty);
        }

        return JsonSerializer.Serialize(fileProperties);
    }
}
