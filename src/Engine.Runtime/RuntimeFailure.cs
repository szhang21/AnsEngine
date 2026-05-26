namespace Engine.Runtime;

public sealed record RuntimeFailure(
    string Stage,
    string Message,
    string? ObjectId = null,
    string? ComponentType = null);
