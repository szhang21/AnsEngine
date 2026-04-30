using System.Numerics;
using Engine.SceneData;

namespace Engine.Editor.App;

public sealed class EditorInspectorInputState
{
    private string? mSyncedObjectId;

    public string ObjectId { get; private set; } = string.Empty;

    public string ObjectName { get; private set; } = string.Empty;

    public string Mesh { get; private set; } = string.Empty;

    public string Material { get; private set; } = string.Empty;

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
            !controller.UpdateObjectResources(currentObjectId, Mesh, Material) ||
            !controller.UpdateObjectTransform(currentObjectId, new SceneFileTransformDefinition(Position, Rotation, Scale)))
        {
            ResetFrom(EditorGuiSnapshotFactory.Create(controller).Inspector);
            return false;
        }

        ResetFrom(EditorGuiSnapshotFactory.Create(controller).Inspector);
        return true;
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
