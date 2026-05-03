using Engine.Asset;
using Engine.Contracts;
using Engine.Core;
using Engine.Platform;
using Engine.Render;
using Engine.Scene;
using Engine.SceneData;
using Engine.SceneData.Abstractions;
using Engine.Scripting;
using System.Diagnostics;
using System.Numerics;
using ContractsProvider = Engine.Contracts.ISceneRenderContractProvider;

namespace Engine.App;

public sealed class RuntimeBootstrap : IRuntimeBootstrap
{
    private const string kSampleMeshCatalogFileName = "mesh-catalog.txt";
    private const string kDefaultSceneFileName = "default.scene.json";
    private const string kScenePathEnvironmentVariableName = "ANS_ENGINE_SCENE_PATH";

    public IApplication Build()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var useNativeWindow = ResolveUseNativeWindow();
        var windowService = new NullWindowService(new WindowConfig(1280, 720, "AnsEngine"), useNativeWindow);
        var inputService = new NullInputService();
        var timeService = new FixedTimeService(new TimeSnapshot(0.016, 0, 60));
        var sceneGraph = new SceneGraphService(runtimeInfo);
        ISceneRuntime sceneRuntime = new SceneRuntimeAdapter(sceneGraph);
        var scriptRuntime = CreateScriptRuntime();
        ContractsProvider renderInputProvider = sceneGraph;
        var meshAssetProvider = CreateMeshAssetProvider();
        var sceneDescriptionLoader = CreateSceneDescriptionLoader();
        var sceneFilePath = ResolveSceneFilePath();
        var renderer = CreateRenderer(useNativeWindow, windowService, runtimeInfo, renderInputProvider, meshAssetProvider);
        var assetService = new NullAssetService(runtimeInfo, windowService, meshAssetProvider);
        return new ApplicationHost(
            windowService,
            renderer,
            sceneRuntime,
            assetService,
            meshAssetProvider,
            sceneDescriptionLoader,
            sceneFilePath,
            inputService,
            timeService,
            scriptRuntime);
    }

    private static ScriptRuntime CreateScriptRuntime()
    {
        var registry = new ScriptRegistry();
        var failure = registry.Register(RotateSelfScript.kScriptId, static () => new RotateSelfScript());
        if (failure is not null)
        {
            throw new InvalidOperationException(failure.Message);
        }

        return new ScriptRuntime(registry);
    }

    private static IRenderer CreateRenderer(
        bool useNativeWindow,
        IWindowService windowService,
        EngineRuntimeInfo runtimeInfo,
        ContractsProvider renderInputProvider,
        IMeshAssetProvider meshAssetProvider)
    {
        return useNativeWindow
            ? new NullRenderer(windowService, runtimeInfo, renderInputProvider, meshAssetProvider)
            : new HeadlessRenderer();
    }

    private static IMeshAssetProvider CreateMeshAssetProvider()
    {
        var sampleAssetRoot = ResolveSampleAssetRoot();
        var catalogPath = Path.Combine(sampleAssetRoot, kSampleMeshCatalogFileName);
        return new DiskMeshAssetProvider(catalogPath);
    }

    private static ISceneDescriptionLoader CreateSceneDescriptionLoader()
    {
        return new JsonSceneDescriptionLoader();
    }

    private static string ResolveSampleAssetRoot()
    {
        return Path.Combine(AppContext.BaseDirectory, "SampleAssets");
    }

    private static string ResolveSceneFilePath()
    {
        var overridePath = Environment.GetEnvironmentVariable(kScenePathEnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(overridePath))
        {
            return overridePath;
        }

        return Path.Combine(AppContext.BaseDirectory, "SampleScenes", kDefaultSceneFileName);
    }

    private static bool ResolveUseNativeWindow()
    {
        var value = Environment.GetEnvironmentVariable("ANS_ENGINE_USE_NATIVE_WINDOW");
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(value, "0", StringComparison.OrdinalIgnoreCase);
    }
}

internal sealed class SceneRuntimeAdapter : ISceneRuntime
{
    private readonly SceneGraphService mSceneGraphService;

    public SceneRuntimeAdapter(SceneGraphService sceneGraphService)
    {
        mSceneGraphService = sceneGraphService ?? throw new ArgumentNullException(nameof(sceneGraphService));
    }

    public void InitializeScene(SceneDescription sceneDescription)
    {
        ArgumentNullException.ThrowIfNull(sceneDescription);
        mSceneGraphService.LoadSceneDescription(sceneDescription);
    }

    public SceneScriptObjectBindResult BindScriptObject(string objectId)
    {
        return mSceneGraphService.BindScriptObject(objectId);
    }

    public void Update(TimeSnapshot time, InputSnapshot input)
    {
        mSceneGraphService.UpdateRuntime(
            new SceneUpdateContext(time.DeltaSeconds, time.TotalSeconds, input.AnyInputDetected));
    }
}

internal sealed class HeadlessRenderer : IRenderer
{
    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
        IsInitialized = true;
    }

    public void RenderFrame()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Renderer is not initialized.");
        }
    }

    public void Shutdown()
    {
        IsInitialized = false;
    }
}

public sealed class ApplicationHost : IApplication
{
    private static readonly SceneMeshRef sBootstrapMesh = new("mesh://cube");
    private readonly IWindowService mWindowService;
    private readonly IRenderer mRenderer;
    private readonly ISceneRuntime mSceneRuntime;
    private readonly IAssetService mAssetService;
    private readonly IMeshAssetProvider mMeshAssetProvider;
    private readonly ISceneDescriptionLoader mSceneDescriptionLoader;
    private readonly string mSceneFilePath;
    private readonly IInputService mInputService;
    private readonly ITimeService mTimeService;
    private readonly ScriptRuntime mScriptRuntime;
    private readonly double? mAutoExitSeconds;

    public ApplicationHost(
        IWindowService windowService,
        IRenderer renderer,
        ISceneRuntime sceneRuntime,
        IAssetService assetService,
        IMeshAssetProvider meshAssetProvider,
        ISceneDescriptionLoader sceneDescriptionLoader,
        string sceneFilePath,
        IInputService inputService,
        ITimeService timeService,
        ScriptRuntime? scriptRuntime = null)
    {
        mWindowService = windowService;
        mRenderer = renderer;
        mSceneRuntime = sceneRuntime;
        mAssetService = assetService;
        mMeshAssetProvider = meshAssetProvider ?? throw new ArgumentNullException(nameof(meshAssetProvider));
        mSceneDescriptionLoader = sceneDescriptionLoader ?? throw new ArgumentNullException(nameof(sceneDescriptionLoader));
        mSceneFilePath = string.IsNullOrWhiteSpace(sceneFilePath)
            ? throw new ArgumentException("Scene file path must not be null or whitespace.", nameof(sceneFilePath))
            : sceneFilePath;
        mInputService = inputService;
        mTimeService = timeService;
        mScriptRuntime = scriptRuntime ?? new ScriptRuntime(new ScriptRegistry());
        mAutoExitSeconds = ResolveAutoExitSeconds();
    }

    public int Run()
    {
        try
        {
            mRenderer.Initialize();
            var uptime = Stopwatch.StartNew();
            var loadResult = mSceneDescriptionLoader.Load(mSceneFilePath);
            if (!loadResult.IsSuccess || loadResult.Scene is null)
            {
                Console.Error.WriteLine($"Scene load failed: {loadResult.Failure?.Kind} - {loadResult.Failure?.Message}");
                return 1;
            }

            mSceneRuntime.InitializeScene(loadResult.Scene);
            var bindResult = BindScripts(loadResult.Scene);
            if (!bindResult.IsSuccess)
            {
                Console.Error.WriteLine($"Script bind failed: {bindResult.Failure?.Kind} - {bindResult.Failure?.Message}");
                return 1;
            }

            _ = mAssetService.Load("bootstrap://placeholder");
            _ = mMeshAssetProvider.GetMesh(sBootstrapMesh);

            while (mWindowService.Exists && !mWindowService.IsCloseRequested)
            {
                mWindowService.ProcessEvents();
                var input = mInputService.GetSnapshot();
                var time = mTimeService.Current;
                mSceneRuntime.Update(time, input);
                var scriptUpdateResult = mScriptRuntime.Update(time.DeltaSeconds, time.TotalSeconds);
                if (!scriptUpdateResult.IsSuccess)
                {
                    Console.Error.WriteLine($"Script update failed: {scriptUpdateResult.Failure?.Kind} - {scriptUpdateResult.Failure?.Message}");
                    return 1;
                }

                mRenderer.RenderFrame();
                mWindowService.Present();

                if (mAutoExitSeconds.HasValue && uptime.Elapsed.TotalSeconds >= mAutoExitSeconds.Value)
                {
                    mWindowService.RequestClose();
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            if (mWindowService.Exists && !mWindowService.IsCloseRequested)
            {
                mWindowService.RequestClose();
            }

            Console.Error.WriteLine($"Fatal error: {ex}");
            return 1;
        }
        finally
        {
            mRenderer.Shutdown();
            mWindowService.Dispose();
        }
    }

    private ScriptBindingResult BindScripts(SceneDescription sceneDescription)
    {
        var bindings = new List<ScriptBindingDescription>();
        foreach (var sceneObject in sceneDescription.Objects)
        {
            foreach (var scriptComponent in sceneObject.ScriptComponents)
            {
                var bindObjectResult = mSceneRuntime.BindScriptObject(sceneObject.ObjectId);
                if (!bindObjectResult.IsSuccess)
                {
                    return ScriptBindingResult.FailureResult(
                        new ScriptFailure(
                            ScriptFailureKind.ScriptFactoryFailed,
                            bindObjectResult.Failure!.Message,
                            scriptComponent.ScriptId,
                            sceneObject.ObjectId));
                }

                bindings.Add(
                    new ScriptBindingDescription(
                        sceneObject.ObjectId,
                        sceneObject.ObjectName,
                        new SceneScriptSelfTransform(bindObjectResult.Handle!),
                        scriptComponent.ScriptId,
                        ConvertProperties(scriptComponent.Properties)));
            }
        }

        return mScriptRuntime.Bind(bindings);
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

    private static double? ResolveAutoExitSeconds()
    {
        var value = Environment.GetEnvironmentVariable("ANS_ENGINE_AUTO_EXIT_SECONDS");
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return double.TryParse(value, out var seconds) && seconds > 0 ? seconds : null;
    }
}

internal sealed class SceneScriptSelfTransform : IScriptSelfTransform
{
    private readonly SceneScriptObjectHandle mHandle;

    public SceneScriptSelfTransform(SceneScriptObjectHandle handle)
    {
        mHandle = handle ?? throw new ArgumentNullException(nameof(handle));
    }

    public SceneTransform LocalTransform => mHandle.LocalTransform;

    public void SetLocalTransform(SceneTransform transform)
    {
        mHandle.SetLocalTransform(transform);
    }
}

public sealed class RotateSelfScript : IScriptBehavior
{
    public const string kScriptId = "RotateSelf";
    private const string kSpeedRadiansPerSecondPropertyName = "speedRadiansPerSecond";

    public void Initialize(ScriptContext context)
    {
        _ = ReadSpeed(context);
    }

    public void Update(ScriptContext context)
    {
        var speed = ReadSpeed(context);
        var rotationDelta = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(context.DeltaSeconds * speed));
        var transform = context.SelfTransform.LocalTransform;
        context.SelfTransform.SetLocalTransform(transform with
        {
            Rotation = Quaternion.Normalize(rotationDelta * transform.Rotation)
        });
    }

    private static double ReadSpeed(ScriptContext context)
    {
        if (!context.Properties.TryGetValue(kSpeedRadiansPerSecondPropertyName, out var value) || !value.IsNumber)
        {
            throw new InvalidOperationException(
                $"Script '{kScriptId}' requires numeric property '{kSpeedRadiansPerSecondPropertyName}'.");
        }

        return value.Number!.Value;
    }
}
