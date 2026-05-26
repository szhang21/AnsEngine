namespace Engine.Runtime;

using Engine.Contracts;
using Engine.Core;
using Engine.Physics;
using Engine.Runtime.Abstractions;
using Engine.Scene;
using Engine.SceneData;
using Engine.Scripting;

public sealed class EngineRuntimeSession
{
    private readonly SceneGraphService mSceneGraphService;
    private readonly ScriptRuntime mScriptRuntime;
    private readonly RuntimePhysicsOrchestrator mPhysicsOrchestrator = new();
    private IReadOnlyList<RuntimeBoundUpdateComponent> mUpdateComponents = Array.Empty<RuntimeBoundUpdateComponent>();
    private PhysicsWorld? mPhysicsWorld;
    private bool mIsInitialized;

    public EngineRuntimeSession()
        : this(new EngineRuntimeInfo("AnsEngine", "0.1.0"), CreateDefaultScriptRuntime())
    {
    }

    public EngineRuntimeSession(EngineRuntimeInfo runtimeInfo, ScriptRuntime scriptRuntime)
    {
        mSceneGraphService = new SceneGraphService(runtimeInfo ?? throw new ArgumentNullException(nameof(runtimeInfo)));
        mScriptRuntime = scriptRuntime ?? throw new ArgumentNullException(nameof(scriptRuntime));
    }

    public ISceneRenderContractProvider SceneRenderProvider => mSceneGraphService;

    public RuntimeSceneSnapshot CreateRuntimeSnapshot()
    {
        return mSceneGraphService.CreateRuntimeSnapshot();
    }

    public RuntimeInitializationResult Initialize(SceneDescription sceneDescription)
    {
        ArgumentNullException.ThrowIfNull(sceneDescription);

        try
        {
            mSceneGraphService.LoadSceneDescription(sceneDescription);
            mPhysicsWorld = ScenePhysicsWorldDefinitionBridge.CreateWorld(sceneDescription);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return RuntimeInitializationResult.FailureResult(
                new RuntimeFailure("Initialize", ex.Message));
        }

        var bindingResult = BindScriptComponents(sceneDescription);
        if (!bindingResult.IsSuccess)
        {
            return RuntimeInitializationResult.FailureResult(bindingResult.Failure!);
        }

        mUpdateComponents = bindingResult.Components;
        mIsInitialized = true;
        return RuntimeInitializationResult.Success();
    }

    public RuntimeTickResult Tick(RuntimeTickContext context)
    {
        if (!mIsInitialized || mPhysicsWorld is null)
        {
            return RuntimeTickResult.FailureResult(
                new RuntimeFailure("Tick", "Runtime session must be initialized before ticking."));
        }

        mSceneGraphService.UpdateRuntime(
            new SceneUpdateContext(
                context.DeltaSeconds,
                context.TotalSeconds,
                context.Input.AnyInputDetected));

        foreach (var updateComponent in mUpdateComponents)
        {
            var updateResult = updateComponent.Component.Update(
                new RuntimeUpdateContext(
                    updateComponent.Owner,
                    context.DeltaSeconds,
                    context.TotalSeconds,
                    context.Input));
            if (!updateResult.IsSuccess)
            {
                var failure = updateResult.Failure!;
                return RuntimeTickResult.FailureResult(
                    new RuntimeFailure(
                        "ScriptUpdate",
                        failure.Message,
                        failure.ObjectId ?? updateComponent.Owner.ObjectId,
                        failure.ComponentType));
            }
        }

        var physicsResult = mPhysicsOrchestrator.ResolveAndWriteBack(mPhysicsWorld, mSceneGraphService);
        if (!physicsResult.IsSuccess)
        {
            return RuntimeTickResult.FailureResult(
                new RuntimeFailure(
                    "PhysicsWriteback",
                    physicsResult.FailureMessage ?? "Runtime physics writeback failed.",
                    physicsResult.ObjectId));
        }

        return RuntimeTickResult.Success(mSceneGraphService.CreateRuntimeSnapshot());
    }

    private RuntimeScriptBindingResult BindScriptComponents(SceneDescription sceneDescription)
    {
        var bindingDescriptions = new List<ScriptUpdateComponentBindingDescription>();
        var owners = new List<IRuntimeObject>();
        foreach (var sceneObject in sceneDescription.Objects)
        {
            foreach (var scriptComponent in sceneObject.ScriptComponents)
            {
                var bindObjectResult = mSceneGraphService.BindScriptObject(sceneObject.ObjectId);
                if (!bindObjectResult.IsSuccess)
                {
                    return RuntimeScriptBindingResult.FailureResult(
                        new RuntimeFailure(
                            "ScriptBinding",
                            bindObjectResult.Failure!.Message,
                            sceneObject.ObjectId,
                            scriptComponent.ScriptId));
                }

                owners.Add(new RuntimeScriptObject(bindObjectResult.Handle!));
                bindingDescriptions.Add(
                    new ScriptUpdateComponentBindingDescription(
                        sceneObject.ObjectId,
                        sceneObject.ObjectName,
                        scriptComponent.ScriptId,
                        ConvertProperties(scriptComponent.Properties)));
            }
        }

        var bindResult = mScriptRuntime.BindUpdateComponents(bindingDescriptions);
        if (!bindResult.IsSuccess)
        {
            return RuntimeScriptBindingResult.FailureResult(
                new RuntimeFailure(
                    "ScriptBinding",
                    bindResult.Failure!.Message,
                    bindResult.Failure.ObjectId,
                    bindResult.Failure.ScriptId));
        }

        var components = bindResult.Components
            .Select((component, index) => new RuntimeBoundUpdateComponent(component, owners[index]))
            .ToArray();
        return RuntimeScriptBindingResult.Success(components);
    }

    private static IReadOnlyDictionary<string, ScriptPropertyValue> ConvertProperties(
        IReadOnlyDictionary<string, SceneScriptPropertyValue> properties)
    {
        var result = new Dictionary<string, ScriptPropertyValue>(StringComparer.Ordinal);
        foreach (var item in properties)
        {
            var value = item.Value;
            if (value.IsNumber)
            {
                result.Add(item.Key, ScriptPropertyValue.FromNumber(value.Number!.Value));
                continue;
            }

            if (value.IsBoolean)
            {
                result.Add(item.Key, ScriptPropertyValue.FromBoolean(value.Boolean!.Value));
                continue;
            }

            if (value.IsString)
            {
                result.Add(item.Key, ScriptPropertyValue.FromString(value.Text ?? string.Empty));
            }
        }

        return result;
    }

    private static ScriptRuntime CreateDefaultScriptRuntime()
    {
        var registry = new ScriptRegistry();
        var failure = registry.Register(RotateSelfScript.kScriptId, static () => new RotateSelfScript());
        failure ??= registry.Register(MoveOnInputScript.kScriptId, static () => new MoveOnInputScript());
        if (failure is not null)
        {
            throw new InvalidOperationException(failure.Message);
        }

        return new ScriptRuntime(registry);
    }

    private sealed record RuntimeScriptBindingResult
    {
        private RuntimeScriptBindingResult(
            IReadOnlyList<RuntimeBoundUpdateComponent> components,
            RuntimeFailure? failure)
        {
            Components = components;
            Failure = failure;
        }

        public bool IsSuccess => Failure is null;

        public IReadOnlyList<RuntimeBoundUpdateComponent> Components { get; }

        public RuntimeFailure? Failure { get; }

        public static RuntimeScriptBindingResult Success(IReadOnlyList<RuntimeBoundUpdateComponent> components)
        {
            return new RuntimeScriptBindingResult(components, null);
        }

        public static RuntimeScriptBindingResult FailureResult(RuntimeFailure failure)
        {
            return new RuntimeScriptBindingResult(Array.Empty<RuntimeBoundUpdateComponent>(), failure);
        }
    }
}
