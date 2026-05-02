namespace Engine.Scripting;

public sealed record ScriptFailure(
    ScriptFailureKind Kind,
    string Message,
    string? ScriptId = null,
    string? ObjectId = null,
    string? PropertyName = null);
