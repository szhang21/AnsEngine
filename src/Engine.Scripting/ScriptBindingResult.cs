namespace Engine.Scripting;

public sealed class ScriptBindingResult
{
    private ScriptBindingResult(ScriptFailure? failure)
    {
        Failure = failure;
    }

    public bool IsSuccess => Failure is null;

    public ScriptFailure? Failure { get; }

    public static ScriptBindingResult Success()
    {
        return new ScriptBindingResult(null);
    }

    public static ScriptBindingResult FailureResult(ScriptFailure failure)
    {
        return new ScriptBindingResult(failure ?? throw new ArgumentNullException(nameof(failure)));
    }
}
