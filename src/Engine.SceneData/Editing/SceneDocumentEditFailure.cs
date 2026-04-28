namespace Engine.SceneData;

public sealed record SceneDocumentEditFailure
{
    public SceneDocumentEditFailure(SceneDocumentEditFailureKind kind, string message)
    {
        if (kind == SceneDocumentEditFailureKind.None)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), "Failure kind must be concrete.");
        }

        Kind = kind;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Failure message must not be null or whitespace.", nameof(message))
            : message;
    }

    public SceneDocumentEditFailureKind Kind { get; }

    public string Message { get; }
}
