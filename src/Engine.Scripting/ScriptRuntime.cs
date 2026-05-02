namespace Engine.Scripting;

public sealed class ScriptRuntime
{
    private readonly ScriptRegistry mRegistry;
    private readonly List<BoundScript> mScripts = new();

    public ScriptRuntime(ScriptRegistry registry)
    {
        mRegistry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public int BoundScriptCount => mScripts.Count;

    public ScriptBindingResult Bind(IReadOnlyList<ScriptBindingDescription> bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);

        var nextScripts = new List<BoundScript>(bindings.Count);
        foreach (var binding in bindings)
        {
            var propertyFailure = ValidateProperties(binding);
            if (propertyFailure is not null)
            {
                return ScriptBindingResult.FailureResult(propertyFailure);
            }

            var createResult = mRegistry.Create(binding.ScriptId);
            if (!createResult.IsSuccess)
            {
                return ScriptBindingResult.FailureResult(createResult.Failure! with { ObjectId = binding.ObjectId });
            }

            var context = new ScriptContext(
                binding.ObjectId,
                binding.ObjectName,
                binding.SelfTransform,
                binding.Properties,
                0.0d,
                0.0d);
            nextScripts.Add(new BoundScript(binding.ScriptId, createResult.Behavior!, context));
        }

        foreach (var script in nextScripts)
        {
            var initializeFailure = Invoke(script, static (behavior, context) => behavior.Initialize(context));
            if (initializeFailure is not null)
            {
                return ScriptBindingResult.FailureResult(initializeFailure);
            }

            script.MarkInitialized();
        }

        mScripts.Clear();
        mScripts.AddRange(nextScripts);
        return ScriptBindingResult.Success();
    }

    public ScriptUpdateResult Update(double deltaSeconds, double totalSeconds)
    {
        foreach (var script in mScripts)
        {
            var context = script.Context.WithTiming(deltaSeconds, totalSeconds);
            var updateFailure = Invoke(script, (behavior, _) => behavior.Update(context));
            if (updateFailure is not null)
            {
                return ScriptUpdateResult.FailureResult(updateFailure);
            }
        }

        return ScriptUpdateResult.Success();
    }

    private static ScriptFailure? ValidateProperties(ScriptBindingDescription binding)
    {
        foreach (var item in binding.Properties)
        {
            if (string.IsNullOrWhiteSpace(item.Key))
            {
                return new ScriptFailure(
                    ScriptFailureKind.InvalidProperty,
                    $"Script id '{binding.ScriptId}' has an empty property name.",
                    binding.ScriptId,
                    binding.ObjectId);
            }

            var value = item.Value;
            if (value.IsNumber && (!value.Number.HasValue || !double.IsFinite(value.Number.Value)))
            {
                return new ScriptFailure(
                    ScriptFailureKind.InvalidProperty,
                    $"Script id '{binding.ScriptId}' property '{item.Key}' is not a finite number.",
                    binding.ScriptId,
                    binding.ObjectId,
                    item.Key);
            }

            if (!value.IsNumber && !value.IsBoolean && !value.IsString)
            {
                return new ScriptFailure(
                    ScriptFailureKind.InvalidProperty,
                    $"Script id '{binding.ScriptId}' property '{item.Key}' has an unsupported value.",
                    binding.ScriptId,
                    binding.ObjectId,
                    item.Key);
            }
        }

        return null;
    }

    private static ScriptFailure? Invoke(
        BoundScript script,
        Action<IScriptBehavior, ScriptContext> invocation)
    {
        try
        {
            invocation(script.Behavior, script.Context);
            return null;
        }
        catch (Exception ex)
        {
            return new ScriptFailure(
                ScriptFailureKind.ScriptException,
                $"Script id '{script.ScriptId}' on object '{script.Context.ObjectId}' failed: {ex.Message}",
                script.ScriptId,
                script.Context.ObjectId);
        }
    }

    private sealed class BoundScript
    {
        public BoundScript(string scriptId, IScriptBehavior behavior, ScriptContext context)
        {
            ScriptId = scriptId;
            Behavior = behavior;
            Context = context;
        }

        public string ScriptId { get; }

        public IScriptBehavior Behavior { get; }

        public ScriptContext Context { get; }

        public bool IsInitialized { get; private set; }

        public void MarkInitialized()
        {
            IsInitialized = true;
        }
    }
}
