namespace Engine.Scripting;

using Engine.Runtime.Abstractions;

public sealed class ScriptRuntime
{
    private readonly ScriptRegistry mRegistry;
    private readonly List<BoundScript> mScripts = new();

    public ScriptRuntime(ScriptRegistry registry)
    {
        mRegistry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public int BoundScriptCount => mScripts.Count;

    public IReadOnlyList<IRuntimeUpdateComponent> BoundUpdateComponents => mScripts;

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
                binding.Self,
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

    public ScriptUpdateComponentBindingResult BindUpdateComponents(
        IReadOnlyList<ScriptUpdateComponentBindingDescription> bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);

        var nextScripts = new List<BoundScript>(bindings.Count);
        foreach (var binding in bindings)
        {
            var propertyFailure = ValidateProperties(binding);
            if (propertyFailure is not null)
            {
                return ScriptUpdateComponentBindingResult.FailureResult(propertyFailure);
            }

            var createResult = mRegistry.Create(binding.ScriptId);
            if (!createResult.IsSuccess)
            {
                return ScriptUpdateComponentBindingResult.FailureResult(
                    createResult.Failure! with { ObjectId = binding.ObjectId });
            }

            var context = new ScriptContext(
                binding.ObjectId,
                binding.ObjectName,
                new RuntimeBindingSelfObject(binding.ObjectId, binding.ObjectName),
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
                return ScriptUpdateComponentBindingResult.FailureResult(initializeFailure);
            }

            script.MarkInitialized();
        }

        mScripts.Clear();
        mScripts.AddRange(nextScripts);
        return ScriptUpdateComponentBindingResult.Success(mScripts);
    }

    public ScriptUpdateResult Update(double deltaSeconds, double totalSeconds)
    {
        return Update(deltaSeconds, totalSeconds, ScriptInputSnapshot.Empty);
    }

    public ScriptUpdateResult Update(double deltaSeconds, double totalSeconds, ScriptInputSnapshot input)
    {
        foreach (var script in mScripts)
        {
            var updateResult = script.Update(
                new RuntimeUpdateContext(
                    script.Context.Self,
                    deltaSeconds,
                    totalSeconds,
                    ConvertInput(input)));
            if (!updateResult.IsSuccess)
            {
                return ScriptUpdateResult.FailureResult(
                    new ScriptFailure(
                        ScriptFailureKind.ScriptException,
                        updateResult.Failure!.Message,
                        script.ScriptId,
                        script.Context.ObjectId));
            }
        }

        return ScriptUpdateResult.Success();
    }

    private static ScriptFailure? ValidateProperties(
        string scriptId,
        string objectId,
        IReadOnlyDictionary<string, ScriptPropertyValue> properties)
    {
        foreach (var item in properties)
        {
            if (string.IsNullOrWhiteSpace(item.Key))
            {
                return new ScriptFailure(
                    ScriptFailureKind.InvalidProperty,
                    $"Script id '{scriptId}' has an empty property name.",
                    scriptId,
                    objectId);
            }

            var value = item.Value;
            if (value.IsNumber && (!value.Number.HasValue || !double.IsFinite(value.Number.Value)))
            {
                return new ScriptFailure(
                    ScriptFailureKind.InvalidProperty,
                    $"Script id '{scriptId}' property '{item.Key}' is not a finite number.",
                    scriptId,
                    objectId,
                    item.Key);
            }

            if (!value.IsNumber && !value.IsBoolean && !value.IsString)
            {
                return new ScriptFailure(
                    ScriptFailureKind.InvalidProperty,
                    $"Script id '{scriptId}' property '{item.Key}' has an unsupported value.",
                    scriptId,
                    objectId,
                    item.Key);
            }
        }

        return null;
    }

    private static ScriptFailure? ValidateProperties(ScriptBindingDescription binding)
    {
        return ValidateProperties(binding.ScriptId, binding.ObjectId, binding.Properties);
    }

    private static ScriptFailure? ValidateProperties(ScriptUpdateComponentBindingDescription binding)
    {
        return ValidateProperties(binding.ScriptId, binding.ObjectId, binding.Properties);
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

    private static RuntimeInputSnapshot ConvertInput(ScriptInputSnapshot input)
    {
        var keys = new List<RuntimeKey>(4);
        if (input.IsKeyDown(ScriptKey.W))
        {
            keys.Add(RuntimeKey.W);
        }

        if (input.IsKeyDown(ScriptKey.A))
        {
            keys.Add(RuntimeKey.A);
        }

        if (input.IsKeyDown(ScriptKey.S))
        {
            keys.Add(RuntimeKey.S);
        }

        if (input.IsKeyDown(ScriptKey.D))
        {
            keys.Add(RuntimeKey.D);
        }

        return keys.Count == 0 ? RuntimeInputSnapshot.Empty : RuntimeInputSnapshot.FromKeys(keys.ToArray());
    }

    private sealed class BoundScript : IRuntimeUpdateComponent
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

        public RuntimeUpdateResult Update(RuntimeUpdateContext context)
        {
            try
            {
                if (Behavior is IRuntimeUpdateComponent runtimeUpdateComponent)
                {
                    return runtimeUpdateComponent.Update(context);
                }

                Behavior.Update(Context.WithFrame(context.DeltaSeconds, context.TotalSeconds, ConvertInput(context.Input)));
                return RuntimeUpdateResult.Success();
            }
            catch (Exception ex)
            {
                return RuntimeUpdateResult.FailureResult(
                    new RuntimeUpdateFailure(
                        $"Script id '{ScriptId}' on object '{Context.ObjectId}' failed: {ex.Message}",
                        Context.ObjectId,
                        ScriptId));
            }
        }

        private static ScriptInputSnapshot ConvertInput(RuntimeInputSnapshot input)
        {
            var keys = new List<ScriptKey>(4);
            if (input.IsKeyDown(RuntimeKey.W))
            {
                keys.Add(ScriptKey.W);
            }

            if (input.IsKeyDown(RuntimeKey.A))
            {
                keys.Add(ScriptKey.A);
            }

            if (input.IsKeyDown(RuntimeKey.S))
            {
                keys.Add(ScriptKey.S);
            }

            if (input.IsKeyDown(RuntimeKey.D))
            {
                keys.Add(ScriptKey.D);
            }

            return keys.Count == 0 ? ScriptInputSnapshot.Empty : ScriptInputSnapshot.FromKeys(keys.ToArray());
        }
    }

    private sealed class RuntimeBindingSelfObject : IScriptSelfObject
    {
        public RuntimeBindingSelfObject(string objectId, string objectName)
        {
            ObjectId = objectId;
            ObjectName = objectName;
        }

        public string ObjectId { get; }

        public string ObjectName { get; }

        public IRuntimeTransformComponent Transform =>
            throw new InvalidOperationException("Runtime component binding does not expose ScriptContext.Self.Transform during initialization.");

        public T? GetComponent<T>() where T : class, IRuntimeComponent
        {
            return null;
        }

        public bool HasComponent<T>() where T : class, IRuntimeComponent
        {
            return false;
        }
    }
}
