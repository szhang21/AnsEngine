namespace Engine.SceneData;

public sealed record SceneFileObjectDefinition(
    string Id,
    string Name,
    string Mesh,
    string? Material,
    SceneFileTransformDefinition? Transform);
