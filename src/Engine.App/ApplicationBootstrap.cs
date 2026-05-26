using Engine.Asset;
using Engine.Contracts;
using Engine.Core;
using Engine.Platform;
using Engine.Render;
using Engine.Runtime;
using Engine.Runtime.Abstractions;
using Engine.SceneData;
using Engine.SceneData.Abstractions;
using System.Diagnostics;
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
        var inputService = CreateInputService(useNativeWindow, windowService);
        var timeService = new FixedTimeService(new TimeSnapshot(0.016, 0, 60));
        var runtimeSession = new EngineRuntimeSessionHost(new EngineRuntimeSession());
        var meshAssetProvider = CreateMeshAssetProvider();
        var sceneDescriptionLoader = CreateSceneDescriptionLoader();
        var sceneFilePath = ResolveSceneFilePath();
        var renderer = CreateRenderer(
            useNativeWindow,
            windowService,
            runtimeInfo,
            runtimeSession.SceneRenderProvider,
            meshAssetProvider);
        var assetService = new NullAssetService(runtimeInfo, windowService, meshAssetProvider);
        return new ApplicationHost(
            windowService,
            renderer,
            runtimeSession,
            assetService,
            meshAssetProvider,
            sceneDescriptionLoader,
            sceneFilePath,
            inputService,
            timeService);
    }

    private static IInputService CreateInputService(bool useNativeWindow, IKeyboardStateProvider keyboardStateProvider)
    {
        return useNativeWindow
            ? new NativeWindowInputService(keyboardStateProvider)
            : new NullInputService();
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

public sealed class EngineRuntimeSessionHost : IRuntimeSessionHost
{
    private readonly EngineRuntimeSession mSession;

    public EngineRuntimeSessionHost(EngineRuntimeSession session)
    {
        mSession = session ?? throw new ArgumentNullException(nameof(session));
    }

    public ContractsProvider SceneRenderProvider => mSession.SceneRenderProvider;

    public RuntimeInitializationResult Initialize(SceneDescription sceneDescription)
    {
        return mSession.Initialize(sceneDescription);
    }

    public RuntimeTickResult Tick(RuntimeTickContext context)
    {
        return mSession.Tick(context);
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
    private readonly IRuntimeSessionHost mRuntimeSession;
    private readonly IAssetService mAssetService;
    private readonly IMeshAssetProvider mMeshAssetProvider;
    private readonly ISceneDescriptionLoader mSceneDescriptionLoader;
    private readonly string mSceneFilePath;
    private readonly IInputService mInputService;
    private readonly ITimeService mTimeService;
    private readonly double? mAutoExitSeconds;

    public ApplicationHost(
        IWindowService windowService,
        IRenderer renderer,
        IRuntimeSessionHost runtimeSession,
        IAssetService assetService,
        IMeshAssetProvider meshAssetProvider,
        ISceneDescriptionLoader sceneDescriptionLoader,
        string sceneFilePath,
        IInputService inputService,
        ITimeService timeService)
    {
        mWindowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        mRenderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        mRuntimeSession = runtimeSession ?? throw new ArgumentNullException(nameof(runtimeSession));
        mAssetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
        mMeshAssetProvider = meshAssetProvider ?? throw new ArgumentNullException(nameof(meshAssetProvider));
        mSceneDescriptionLoader = sceneDescriptionLoader ?? throw new ArgumentNullException(nameof(sceneDescriptionLoader));
        mSceneFilePath = string.IsNullOrWhiteSpace(sceneFilePath)
            ? throw new ArgumentException("Scene file path must not be null or whitespace.", nameof(sceneFilePath))
            : sceneFilePath;
        mInputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        mTimeService = timeService ?? throw new ArgumentNullException(nameof(timeService));
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

            var initializeResult = mRuntimeSession.Initialize(loadResult.Scene);
            if (!initializeResult.IsSuccess)
            {
                Console.Error.WriteLine(
                    $"Runtime initialize failed: {initializeResult.Failure?.Stage} - {initializeResult.Failure?.Message}");
                return 1;
            }

            _ = mAssetService.Load("bootstrap://placeholder");
            _ = mMeshAssetProvider.GetMesh(sBootstrapMesh);

            while (mWindowService.Exists && !mWindowService.IsCloseRequested)
            {
                mWindowService.ProcessEvents();
                var input = mInputService.GetSnapshot();
                var time = mTimeService.Current;
                var tickResult = mRuntimeSession.Tick(
                    new RuntimeTickContext(
                        time.DeltaSeconds,
                        time.TotalSeconds,
                        ConvertInput(input)));
                if (!tickResult.IsSuccess)
                {
                    Console.Error.WriteLine(
                        $"Runtime tick failed: {tickResult.Failure?.Stage} - {tickResult.Failure?.Message}");
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

    private static RuntimeInputSnapshot ConvertInput(InputSnapshot input)
    {
        var keys = new List<RuntimeKey>(4);
        if (input.IsKeyDown(EngineKey.W))
        {
            keys.Add(RuntimeKey.W);
        }

        if (input.IsKeyDown(EngineKey.A))
        {
            keys.Add(RuntimeKey.A);
        }

        if (input.IsKeyDown(EngineKey.S))
        {
            keys.Add(RuntimeKey.S);
        }

        if (input.IsKeyDown(EngineKey.D))
        {
            keys.Add(RuntimeKey.D);
        }

        return keys.Count == 0 ? RuntimeInputSnapshot.Empty : RuntimeInputSnapshot.FromKeys(keys.ToArray());
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
