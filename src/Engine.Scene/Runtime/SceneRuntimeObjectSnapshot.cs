using Engine.Contracts;

namespace Engine.Scene;

public sealed record SceneRuntimeObjectSnapshot(
    int NodeId,
    string ObjectId,
    string ObjectName,
    bool HasTransform,
    SceneTransform? LocalTransform,
    bool HasMeshRenderer,
    SceneMeshRef? Mesh,
    SceneMaterialRef? Material);
