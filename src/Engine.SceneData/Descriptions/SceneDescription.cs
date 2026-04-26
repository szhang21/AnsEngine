namespace Engine.SceneData;

public sealed record SceneDescription(
    string SceneId,
    string SceneName,
    SceneCameraDescription Camera,
    IReadOnlyList<SceneObjectDescription> Objects);
