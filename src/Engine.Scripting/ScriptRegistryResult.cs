namespace Engine.Scripting;

public sealed record ScriptRegistryResult
{
    private ScriptRegistryResult(IScriptBehavior? behavior, ScriptFailure? failure)
    {
        Behavior = behavior;
        Failure = failure;
    }

    public bool IsSuccess => Failure is null;

    public IScriptBehavior? Behavior { get; }

    public ScriptFailure? Failure { get; }

    public static ScriptRegistryResult Success(IScriptBehavior behavior)
    {
        return new ScriptRegistryResult(behavior ?? throw new ArgumentNullException(nameof(behavior)), null);
    }

    public static ScriptRegistryResult FailureResult(ScriptFailure failure)
    {
        return new ScriptRegistryResult(null, failure ?? throw new ArgumentNullException(nameof(failure)));
    }
}
