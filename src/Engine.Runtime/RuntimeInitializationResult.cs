namespace Engine.Runtime;

public sealed record RuntimeInitializationResult
{
    private static readonly RuntimeInitializationResult sSuccess = new(true, null);

    private RuntimeInitializationResult(bool isSuccess, RuntimeFailure? failure)
    {
        IsSuccess = isSuccess;
        Failure = failure;
    }

    public bool IsSuccess { get; }

    public RuntimeFailure? Failure { get; }

    public static RuntimeInitializationResult Success()
    {
        return sSuccess;
    }

    public static RuntimeInitializationResult FailureResult(RuntimeFailure failure)
    {
        return new RuntimeInitializationResult(false, failure ?? throw new ArgumentNullException(nameof(failure)));
    }
}
