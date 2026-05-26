namespace Engine.Runtime.Abstractions;

public sealed record RuntimeUpdateResult
{
    private static readonly RuntimeUpdateResult sSuccess = new(true, null);

    private RuntimeUpdateResult(bool isSuccess, RuntimeUpdateFailure? failure)
    {
        IsSuccess = isSuccess;
        Failure = failure;
    }

    public bool IsSuccess { get; }

    public RuntimeUpdateFailure? Failure { get; }

    public static RuntimeUpdateResult Success()
    {
        return sSuccess;
    }

    public static RuntimeUpdateResult FailureResult(RuntimeUpdateFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);

        return new RuntimeUpdateResult(false, failure);
    }
}
