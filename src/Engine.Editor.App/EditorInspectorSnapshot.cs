using System.Numerics;

namespace Engine.Editor.App;

public sealed record EditorInspectorSnapshot(
    bool HasSelectedObject,
    EditorInspectorObjectGroupSnapshot Object,
    EditorInspectorTransformGroupSnapshot Transform,
    EditorInspectorMeshRendererGroupSnapshot MeshRenderer,
    EditorInspectorScriptsGroupSnapshot Scripts,
    EditorInspectorRigidBodyGroupSnapshot RigidBody,
    EditorInspectorBoxColliderGroupSnapshot BoxCollider,
    EditorInspectorPhysicsParticipationSnapshot PhysicsParticipation,
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

public sealed record EditorInspectorScriptsGroupSnapshot(
    string Title,
    IReadOnlyList<EditorInspectorScriptSnapshot> Scripts,
    string EmptyMessage);

public sealed record EditorInspectorScriptSnapshot(
    string ScriptId,
    string PropertiesJson);

public sealed record EditorInspectorRigidBodyGroupSnapshot(
    string Title,
    bool HasRigidBody,
    string BodyType,
    double Mass,
    string EmptyMessage);

public sealed record EditorInspectorBoxColliderGroupSnapshot(
    string Title,
    bool HasBoxCollider,
    Vector3 Size,
    Vector3 Center,
    string EmptyMessage);

public sealed record EditorInspectorPhysicsParticipationSnapshot(
    string Title,
    bool HasTransform,
    bool HasRigidBody,
    bool HasBoxCollider)
{
    public bool IsPhysicsReady => HasTransform && HasRigidBody && HasBoxCollider;
}
