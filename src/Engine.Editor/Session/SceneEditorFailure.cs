namespace Engine.Editor;

public sealed record SceneEditorFailure
{
    public SceneEditorFailure(
        SceneEditorFailureKind kind,
        string message,
        string? path = null,
        string? objectId = null)
    {
        if (kind == SceneEditorFailureKind.None)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), "Failure kind must be concrete.");
        }

        Kind = kind;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Failure message must not be null or whitespace.", nameof(message))
            : message;
        Path = path;
        ObjectId = objectId;
    }

    public SceneEditorFailureKind Kind { get; }

    public string Message { get; }

    public string? Path { get; }

    public string? ObjectId { get; }
}
