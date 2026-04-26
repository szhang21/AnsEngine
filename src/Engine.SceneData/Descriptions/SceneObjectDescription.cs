using Engine.Contracts;

namespace Engine.SceneData;

public sealed record SceneObjectDescription(
    string ObjectId,
    string ObjectName,
    SceneMeshRef Mesh,
    SceneMaterialRef Material,
    SceneTransformDescription LocalTransform);
