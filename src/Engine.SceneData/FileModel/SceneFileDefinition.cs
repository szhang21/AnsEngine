namespace Engine.SceneData;

public sealed record SceneFileDefinition(
    string Id,
    string Name,
    SceneFileCameraDefinition? Camera,
    IReadOnlyList<SceneFileObjectDefinition> Objects);
