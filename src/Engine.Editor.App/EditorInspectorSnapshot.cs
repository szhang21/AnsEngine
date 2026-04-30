using System.Numerics;

namespace Engine.Editor.App;

public sealed record EditorInspectorSnapshot(
    bool HasSelectedObject,
    string ObjectId,
    string ObjectName,
    string Mesh,
    string Material,
    Vector3 Position,
    Quaternion Rotation,
    Vector3 Scale,
    string EmptyMessage);
