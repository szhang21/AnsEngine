using System.Numerics;
using System.Text.Json;
using Engine.SceneData;

namespace Engine.Editor.App;

public sealed class EditorInspectorInputState
{
    private string? mSyncedObjectId;

    public string ObjectId { get; private set; } = string.Empty;

    public string ObjectName { get; private set; } = string.Empty;

    public string Mesh { get; private set; } = string.Empty;

    public string Material { get; private set; } = string.Empty;

    public bool HasMeshRenderer { get; private set; }

    public string FirstScriptId { get; private set; } = string.Empty;

    public string FirstScriptPropertiesJson { get; private set; } = "{}";

    public bool HasScript { get; private set; }

    public string RigidBodyType { get; private set; } = "Dynamic";

    public double RigidBodyMass { get; private set; } = 1.0d;

    public bool HasRigidBody { get; private set; }

    public Vector3 BoxColliderSize { get; private set; } = Vector3.One;

    public Vector3 BoxColliderCenter { get; private set; }

    public bool HasBoxCollider { get; private set; }

    public Vector3 Position { get; private set; }

    public Quaternion Rotation { get; private set; } = Quaternion.Identity;

    public Vector3 Scale { get; private set; } = Vector3.One;

    public void SyncFrom(EditorInspectorSnapshot snapshot)
    {
        if (!snapshot.HasSelectedObject)
        {
            ResetFrom(snapshot);
            return;
        }

        if (string.Equals(mSyncedObjectId, snapshot.ObjectId, StringComparison.Ordinal))
        {
            return;
        }

        ResetFrom(snapshot);
    }

    public void ResetFrom(EditorInspectorSnapshot snapshot)
    {
        if (!snapshot.HasSelectedObject)
        {
            mSyncedObjectId = null;
            ObjectId = string.Empty;
            ObjectName = string.Empty;
            Mesh = string.Empty;
            Material = string.Empty;
            HasMeshRenderer = false;
            FirstScriptId = string.Empty;
            FirstScriptPropertiesJson = "{}";
            HasScript = false;
            RigidBodyType = "Dynamic";
            RigidBodyMass = 1.0d;
            HasRigidBody = false;
            BoxColliderSize = Vector3.One;
            BoxColliderCenter = Vector3.Zero;
            HasBoxCollider = false;
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
            return;
        }

        mSyncedObjectId = snapshot.ObjectId;
        ObjectId = snapshot.ObjectId;
        ObjectName = snapshot.ObjectName;
        Mesh = snapshot.Mesh;
        Material = snapshot.Material;
        HasMeshRenderer = snapshot.MeshRenderer.HasMeshRenderer;
        var firstScript = snapshot.Scripts.Scripts.FirstOrDefault();
        FirstScriptId = firstScript?.ScriptId ?? "MoveOnInput";
        FirstScriptPropertiesJson = firstScript?.PropertiesJson ?? "{}";
        HasScript = firstScript is not null;
        RigidBodyType = snapshot.RigidBody.BodyType;
        RigidBodyMass = snapshot.RigidBody.Mass;
        HasRigidBody = snapshot.RigidBody.HasRigidBody;
        BoxColliderSize = snapshot.BoxCollider.Size;
        BoxColliderCenter = snapshot.BoxCollider.Center;
        HasBoxCollider = snapshot.BoxCollider.HasBoxCollider;
        Position = snapshot.Position;
        Rotation = snapshot.Rotation;
        Scale = snapshot.Scale;
    }

    public void SetTextValues(string objectId, string objectName, string mesh, string material)
    {
        ObjectId = objectId;
        ObjectName = objectName;
        Mesh = mesh;
        Material = material;
    }

    public void SetScriptValues(string scriptId, string propertiesJson)
    {
        FirstScriptId = scriptId;
        FirstScriptPropertiesJson = propertiesJson;
        HasScript = true;
    }

    public void SetRigidBodyValues(string bodyType, double mass)
    {
        RigidBodyType = bodyType;
        RigidBodyMass = mass;
        HasRigidBody = true;
    }

    public void SetBoxColliderValues(Vector3 size, Vector3 center)
    {
        BoxColliderSize = size;
        BoxColliderCenter = center;
        HasBoxCollider = true;
    }

    public void ClearScript()
    {
        HasScript = false;
    }

    public void ClearRigidBody()
    {
        HasRigidBody = false;
    }

    public void ClearBoxCollider()
    {
        HasBoxCollider = false;
    }

    public void SetTransformValues(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public bool Apply(EditorAppController controller, EditorInspectorSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (!snapshot.HasSelectedObject)
        {
            controller.SetLastError("No object is selected.");
            return false;
        }

        var currentObjectId = snapshot.ObjectId;
        if (controller.Session.Objects.Any(item =>
                !string.Equals(item.ObjectId, currentObjectId, StringComparison.Ordinal) &&
                string.Equals(item.ObjectId, ObjectId, StringComparison.Ordinal)))
        {
            controller.SetLastError($"Scene object id '{ObjectId}' is duplicated.");
            ResetFrom(EditorGuiSnapshotFactory.Create(controller).Inspector);
            return false;
        }

        if (!IsFinite(Position) || !IsFinite(Rotation) || !IsFinite(Scale))
        {
            controller.SetLastError($"Scene object '{currentObjectId}' contains non-finite transform values.");
            ResetFrom(EditorGuiSnapshotFactory.Create(controller).Inspector);
            return false;
        }

        IReadOnlyDictionary<string, SceneFileScriptPropertyValue>? scriptProperties = null;
        if (HasScript &&
            !TryReadScriptProperties(FirstScriptPropertiesJson, out scriptProperties, out var error))
        {
            controller.SetLastError(error);
            ResetFrom(EditorGuiSnapshotFactory.Create(controller).Inspector);
            return false;
        }

        if (!string.Equals(currentObjectId, ObjectId, StringComparison.Ordinal))
        {
            if (!controller.UpdateObjectId(currentObjectId, ObjectId))
            {
                ResetFrom(EditorGuiSnapshotFactory.Create(controller).Inspector);
                return false;
            }

            currentObjectId = ObjectId;
        }

        if (!controller.UpdateObjectName(currentObjectId, ObjectName) ||
            !controller.UpdateObjectTransformComponent(currentObjectId, new SceneFileTransformDefinition(Position, Rotation, Scale)))
        {
            ResetFrom(EditorGuiSnapshotFactory.Create(controller).Inspector);
            return false;
        }

        if (HasMeshRenderer &&
            !controller.UpdateObjectMeshRendererComponent(currentObjectId, Mesh, Material))
        {
            ResetFrom(EditorGuiSnapshotFactory.Create(controller).Inspector);
            return false;
        }

        if (!ApplyScript(controller, snapshot, currentObjectId, scriptProperties) ||
            !ApplyRigidBody(controller, currentObjectId) ||
            !ApplyBoxCollider(controller, currentObjectId))
        {
            ResetFrom(EditorGuiSnapshotFactory.Create(controller).Inspector);
            return false;
        }

        ResetFrom(EditorGuiSnapshotFactory.Create(controller).Inspector);
        return true;
    }

    public bool AddScriptComponent(EditorAppController controller, EditorInspectorSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(controller);
        if (!snapshot.HasSelectedObject)
        {
            controller.SetLastError("No object is selected.");
            return false;
        }

        return controller.UpdateObjectScriptComponent(
            snapshot.ObjectId,
            new SceneFileScriptComponentDefinition("MoveOnInput", null));
    }

    public bool RemoveFirstScriptComponent(EditorAppController controller, EditorInspectorSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(controller);
        if (!snapshot.HasSelectedObject || snapshot.Scripts.Scripts.Count == 0)
        {
            controller.SetLastError("No Script component is available to remove.");
            return false;
        }

        return controller.RemoveObjectScriptComponent(snapshot.ObjectId, snapshot.Scripts.Scripts[0].ScriptId);
    }

    public bool AddRigidBodyComponent(EditorAppController controller, EditorInspectorSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(controller);
        if (!snapshot.HasSelectedObject)
        {
            controller.SetLastError("No object is selected.");
            return false;
        }

        return controller.UpdateObjectRigidBodyComponent(
            snapshot.ObjectId,
            new SceneFileRigidBodyComponentDefinition("Dynamic", 1.0d));
    }

    public bool RemoveRigidBodyComponent(EditorAppController controller, EditorInspectorSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(controller);
        if (!snapshot.HasSelectedObject)
        {
            controller.SetLastError("No object is selected.");
            return false;
        }

        return controller.RemoveObjectRigidBodyComponent(snapshot.ObjectId);
    }

    public bool AddBoxColliderComponent(EditorAppController controller, EditorInspectorSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(controller);
        if (!snapshot.HasSelectedObject)
        {
            controller.SetLastError("No object is selected.");
            return false;
        }

        return controller.UpdateObjectBoxColliderComponent(
            snapshot.ObjectId,
            new SceneFileBoxColliderComponentDefinition(Vector3.One, Vector3.Zero));
    }

    public bool RemoveBoxColliderComponent(EditorAppController controller, EditorInspectorSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(controller);
        if (!snapshot.HasSelectedObject)
        {
            controller.SetLastError("No object is selected.");
            return false;
        }

        return controller.RemoveObjectBoxColliderComponent(snapshot.ObjectId);
    }

    private bool ApplyScript(
        EditorAppController controller,
        EditorInspectorSnapshot snapshot,
        string objectId,
        IReadOnlyDictionary<string, SceneFileScriptPropertyValue>? scriptProperties)
    {
        var existingScriptId = snapshot.Scripts.Scripts.FirstOrDefault()?.ScriptId ?? string.Empty;
        if (!HasScript)
        {
            return string.IsNullOrWhiteSpace(existingScriptId) ||
                   controller.RemoveObjectScriptComponent(objectId, existingScriptId);
        }

        if (!string.IsNullOrWhiteSpace(existingScriptId) &&
            !string.Equals(existingScriptId, FirstScriptId, StringComparison.Ordinal) &&
            !controller.RemoveObjectScriptComponent(objectId, existingScriptId))
        {
            return false;
        }

        return controller.UpdateObjectScriptComponent(
            objectId,
            new SceneFileScriptComponentDefinition(FirstScriptId, scriptProperties));
    }

    private bool ApplyRigidBody(EditorAppController controller, string objectId)
    {
        return HasRigidBody
            ? controller.UpdateObjectRigidBodyComponent(
                objectId,
                new SceneFileRigidBodyComponentDefinition(RigidBodyType, RigidBodyMass))
            : controller.RemoveObjectRigidBodyComponent(objectId);
    }

    private bool ApplyBoxCollider(EditorAppController controller, string objectId)
    {
        return HasBoxCollider
            ? controller.UpdateObjectBoxColliderComponent(
                objectId,
                new SceneFileBoxColliderComponentDefinition(BoxColliderSize, BoxColliderCenter))
            : controller.RemoveObjectBoxColliderComponent(objectId);
    }

    private static bool TryReadScriptProperties(
        string propertiesJson,
        out IReadOnlyDictionary<string, SceneFileScriptPropertyValue> properties,
        out string error)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(propertiesJson) ? "{}" : propertiesJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                properties = new Dictionary<string, SceneFileScriptPropertyValue>(StringComparer.Ordinal);
                error = "Script properties must be a JSON object.";
                return false;
            }

            var values = new Dictionary<string, SceneFileScriptPropertyValue>(StringComparer.Ordinal);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                values[property.Name] = SceneFileScriptPropertyValue.FromJsonElement(property.Value);
            }

            properties = values;
            error = string.Empty;
            return true;
        }
        catch (JsonException ex)
        {
            properties = new Dictionary<string, SceneFileScriptPropertyValue>(StringComparer.Ordinal);
            error = $"Script properties JSON is invalid: {ex.Message}";
            return false;
        }
    }

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.X) &&
               float.IsFinite(value.Y) &&
               float.IsFinite(value.Z);
    }

    private static bool IsFinite(Quaternion value)
    {
        return float.IsFinite(value.X) &&
               float.IsFinite(value.Y) &&
               float.IsFinite(value.Z) &&
               float.IsFinite(value.W);
    }
}
