namespace Engine.Scene;

public sealed record SceneTransformWriteFailure
{
    public SceneTransformWriteFailure(
        SceneTransformWriteFailureKind kind,
        string message,
        string objectId)
    {
        if (kind == SceneTransformWriteFailureKind.None)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), "Scene transform write failure kind must be concrete.");
        }

        Kind = kind;
        Message = string.IsNullOrWhiteSpace(message) ? kind.ToString() : message;
        ObjectId = string.IsNullOrWhiteSpace(objectId) ? "<unknown>" : objectId;
    }

    public SceneTransformWriteFailureKind Kind { get; }

    public string Message { get; }

    public string ObjectId { get; }
}
