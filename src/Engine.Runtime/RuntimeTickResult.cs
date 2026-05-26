namespace Engine.Runtime;

using Engine.Scene;

public sealed record RuntimeTickResult
{
    private RuntimeTickResult(bool isSuccess, RuntimeSceneSnapshot? snapshot, RuntimeFailure? failure)
    {
        IsSuccess = isSuccess;
        Snapshot = snapshot;
        Failure = failure;
    }

    public bool IsSuccess { get; }

    public RuntimeSceneSnapshot? Snapshot { get; }

    public RuntimeFailure? Failure { get; }

    public static RuntimeTickResult Success(RuntimeSceneSnapshot snapshot)
    {
        return new RuntimeTickResult(true, snapshot ?? throw new ArgumentNullException(nameof(snapshot)), null);
    }

    public static RuntimeTickResult FailureResult(RuntimeFailure failure)
    {
        return new RuntimeTickResult(false, null, failure ?? throw new ArgumentNullException(nameof(failure)));
    }
}
