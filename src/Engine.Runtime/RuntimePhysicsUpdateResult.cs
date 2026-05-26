namespace Engine.Runtime;

internal sealed record RuntimePhysicsUpdateResult
{
    private RuntimePhysicsUpdateResult(bool isSuccess, string? failureMessage, string? objectId)
    {
        IsSuccess = isSuccess;
        FailureMessage = failureMessage;
        ObjectId = objectId;
    }

    public bool IsSuccess { get; }

    public string? FailureMessage { get; }

    public string? ObjectId { get; }

    public static RuntimePhysicsUpdateResult Success()
    {
        return new RuntimePhysicsUpdateResult(true, null, null);
    }

    public static RuntimePhysicsUpdateResult FailureResult(string failureMessage, string? objectId = null)
    {
        return new RuntimePhysicsUpdateResult(false, failureMessage, objectId);
    }
}
