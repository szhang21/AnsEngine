namespace Engine.Scripting;

public sealed class ScriptRegistry
{
    private readonly Dictionary<string, Func<IScriptBehavior>> mFactories = new(StringComparer.Ordinal);

    public ScriptFailure? Register(string scriptId, Func<IScriptBehavior> factory)
    {
        if (string.IsNullOrWhiteSpace(scriptId))
        {
            return new ScriptFailure(ScriptFailureKind.MissingScriptId, "Script id is required.", scriptId);
        }

        ArgumentNullException.ThrowIfNull(factory);
        if (mFactories.ContainsKey(scriptId))
        {
            return new ScriptFailure(
                ScriptFailureKind.DuplicateScriptId,
                $"Script id '{scriptId}' is already registered.",
                scriptId);
        }

        mFactories.Add(scriptId, factory);
        return null;
    }

    public ScriptRegistryResult Create(string scriptId)
    {
        if (!mFactories.TryGetValue(scriptId, out var factory))
        {
            return ScriptRegistryResult.FailureResult(
                new ScriptFailure(
                    ScriptFailureKind.MissingScriptId,
                    $"Script id '{scriptId}' is not registered.",
                    scriptId));
        }

        try
        {
            var behavior = factory();
            return behavior is null
                ? ScriptRegistryResult.FailureResult(
                    new ScriptFailure(
                        ScriptFailureKind.ScriptFactoryFailed,
                        $"Script id '{scriptId}' factory returned null.",
                        scriptId))
                : ScriptRegistryResult.Success(behavior);
        }
        catch (Exception ex)
        {
            return ScriptRegistryResult.FailureResult(
                new ScriptFailure(
                    ScriptFailureKind.ScriptFactoryFailed,
                    $"Script id '{scriptId}' factory failed: {ex.Message}",
                    scriptId));
        }
    }
}
