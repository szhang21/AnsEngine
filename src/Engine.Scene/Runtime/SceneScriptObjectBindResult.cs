namespace Engine.Scene;

public sealed class SceneScriptObjectBindResult
{
    private SceneScriptObjectBindResult(SceneScriptObjectHandle? handle, SceneScriptObjectBindFailure? failure)
    {
        Handle = handle;
        Failure = failure;
    }

    public bool IsSuccess => Failure is null;

    public SceneScriptObjectHandle? Handle { get; }

    public SceneScriptObjectBindFailure? Failure { get; }

    public static SceneScriptObjectBindResult Success(SceneScriptObjectHandle handle)
    {
        return new SceneScriptObjectBindResult(handle ?? throw new ArgumentNullException(nameof(handle)), null);
    }

    public static SceneScriptObjectBindResult FailureResult(SceneScriptObjectBindFailure failure)
    {
        return new SceneScriptObjectBindResult(null, failure ?? throw new ArgumentNullException(nameof(failure)));
    }
}
