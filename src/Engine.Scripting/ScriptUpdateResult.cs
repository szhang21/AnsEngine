namespace Engine.Scripting;

public sealed class ScriptUpdateResult
{
    private ScriptUpdateResult(ScriptFailure? failure)
    {
        Failure = failure;
    }

    public bool IsSuccess => Failure is null;

    public ScriptFailure? Failure { get; }

    public static ScriptUpdateResult Success()
    {
        return new ScriptUpdateResult(null);
    }

    public static ScriptUpdateResult FailureResult(ScriptFailure failure)
    {
        return new ScriptUpdateResult(failure ?? throw new ArgumentNullException(nameof(failure)));
    }
}
