namespace Engine.SceneData;

public sealed record SceneDescriptionLoadFailure
{
    public SceneDescriptionLoadFailure(
        SceneDescriptionLoadFailureKind kind,
        string message,
        string path,
        int? lineNumber = null)
    {
        if (kind == SceneDescriptionLoadFailureKind.None)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), "Failure kind must not be None.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Failure message must not be null or whitespace.", nameof(message));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must not be null or whitespace.", nameof(path));
        }

        Kind = kind;
        Message = message;
        Path = path;
        LineNumber = lineNumber;
    }

    public SceneDescriptionLoadFailureKind Kind { get; }

    public string Message { get; }

    public string Path { get; }

    public int? LineNumber { get; }
}
