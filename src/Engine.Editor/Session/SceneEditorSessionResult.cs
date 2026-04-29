namespace Engine.Editor;

public sealed record SceneEditorSessionResult
{
    private SceneEditorSessionResult(SceneEditorFailure? failure)
    {
        Failure = failure;
    }

    public bool IsSuccess => Failure is null;

    public SceneEditorFailure? Failure { get; }

    public static SceneEditorSessionResult Success()
    {
        return new SceneEditorSessionResult(failure: null);
    }

    public static SceneEditorSessionResult FailureResult(SceneEditorFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new SceneEditorSessionResult(failure);
    }
}
