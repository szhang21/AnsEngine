namespace Engine.Runtime.Abstractions;

public sealed record RuntimeUpdateFailure(
    string Message,
    string? ObjectId = null,
    string? ComponentType = null);
