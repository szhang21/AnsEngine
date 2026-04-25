namespace Engine.Contracts;

public sealed record MeshAssetLoadFailure
{
    public MeshAssetLoadFailure(MeshAssetLoadFailureKind kind, string message)
    {
        if (kind == MeshAssetLoadFailureKind.None)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Failure kind must not be None.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Failure message must not be null or whitespace.", nameof(message));
        }

        Kind = kind;
        Message = message;
    }

    public MeshAssetLoadFailureKind Kind { get; }

    public string Message { get; }
}
