namespace Engine.SceneData;

public sealed record SceneFileDocument(
    string Version,
    SceneFileDefinition Scene);
