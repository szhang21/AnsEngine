namespace Engine.Scene;

public sealed record SceneTransformWriteResult
{
    private SceneTransformWriteResult(bool isSuccess, SceneTransformWriteFailure? failure)
    {
        IsSuccess = isSuccess;
        Failure = failure;
    }

    public bool IsSuccess { get; }

    public SceneTransformWriteFailure? Failure { get; }

    public static SceneTransformWriteResult Success()
    {
        return new SceneTransformWriteResult(true, null);
    }

    public static SceneTransformWriteResult FailureResult(SceneTransformWriteFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new SceneTransformWriteResult(false, failure);
    }
}
