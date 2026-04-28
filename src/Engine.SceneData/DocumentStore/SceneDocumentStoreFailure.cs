namespace Engine.SceneData;

public sealed record SceneDocumentStoreFailure
{
    public SceneDocumentStoreFailure(
        SceneDocumentStoreFailureKind kind,
        string message,
        string path,
        int? lineNumber = null)
    {
        if (kind == SceneDocumentStoreFailureKind.None)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), "Failure kind must be concrete.");
        }

        Kind = kind;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Failure message must not be null or whitespace.", nameof(message))
            : message;
        Path = string.IsNullOrWhiteSpace(path) ? "<unknown>" : path;
        LineNumber = lineNumber;
    }

    public SceneDocumentStoreFailureKind Kind { get; }

    public string Message { get; }

    public string Path { get; }

    public int? LineNumber { get; }
}
