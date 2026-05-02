namespace Engine.Scene;

public sealed record SceneScriptObjectBindFailure(
    SceneScriptObjectBindFailureKind Kind,
    string Message,
    string ObjectId);
