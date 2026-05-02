using System.Numerics;

namespace Engine.Editor.App;

public sealed record EditorInspectorSnapshot(
    bool HasSelectedObject,
    EditorInspectorObjectGroupSnapshot Object,
    EditorInspectorTransformGroupSnapshot Transform,
    EditorInspectorMeshRendererGroupSnapshot MeshRenderer,
    string EmptyMessage)
{
    public string ObjectId => Object.ObjectId;

    public string ObjectName => Object.ObjectName;

    public string Mesh => MeshRenderer.Mesh;

    public string Material => MeshRenderer.Material;

    public Vector3 Position => Transform.Position;

    public Quaternion Rotation => Transform.Rotation;

    public Vector3 Scale => Transform.Scale;
}

public sealed record EditorInspectorObjectGroupSnapshot(
    string Title,
    string ObjectId,
    string ObjectName);

public sealed record EditorInspectorTransformGroupSnapshot(
    string Title,
    bool HasTransform,
    Vector3 Position,
    Quaternion Rotation,
    Vector3 Scale);

public sealed record EditorInspectorMeshRendererGroupSnapshot(
    string Title,
    bool HasMeshRenderer,
    string Mesh,
    string Material,
    string EmptyMessage);
