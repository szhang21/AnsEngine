namespace Engine.Scripting;

using Engine.Runtime.Abstractions;

public sealed class ScriptUpdateComponentBindingResult
{
    private ScriptUpdateComponentBindingResult(
        IReadOnlyList<IRuntimeUpdateComponent> components,
        ScriptFailure? failure)
    {
        Components = components;
        Failure = failure;
    }

    public bool IsSuccess => Failure is null;

    public IReadOnlyList<IRuntimeUpdateComponent> Components { get; }

    public ScriptFailure? Failure { get; }

    public static ScriptUpdateComponentBindingResult Success(IReadOnlyList<IRuntimeUpdateComponent> components)
    {
        return new ScriptUpdateComponentBindingResult(
            components ?? throw new ArgumentNullException(nameof(components)),
            null);
    }

    public static ScriptUpdateComponentBindingResult FailureResult(ScriptFailure failure)
    {
        return new ScriptUpdateComponentBindingResult(
            Array.Empty<IRuntimeUpdateComponent>(),
            failure ?? throw new ArgumentNullException(nameof(failure)));
    }
}
