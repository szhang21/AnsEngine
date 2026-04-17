using Engine.Asset;
using Engine.Contracts;
using Engine.Core;
using Engine.Platform;
using Engine.Render;
using Engine.Scene;
using System.Diagnostics;
using ContractsProvider = Engine.Contracts.ISceneRenderContractProvider;

namespace Engine.App;

public sealed class RuntimeBootstrap : IRuntimeBootstrap
{
    public IApplication Build()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var useNativeWindow = ResolveUseNativeWindow();
        var windowService = new NullWindowService(new WindowConfig(1280, 720, "AnsEngine"), useNativeWindow);
        var inputService = new NullInputService();
        var timeService = new FixedTimeService(new TimeSnapshot(0.016, 0, 60));
        var sceneGraph = new SceneGraphService(runtimeInfo);
        ISceneRuntime sceneRuntime = new SceneRuntimeAdapter(sceneGraph);
        ContractsProvider renderInputProvider = sceneGraph;
        var renderer = CreateRenderer(useNativeWindow, windowService, runtimeInfo, renderInputProvider);
        var assetService = new NullAssetService(runtimeInfo, windowService);
        return new ApplicationHost(windowService, renderer, sceneRuntime, assetService, inputService, timeService);
    }

    private static IRenderer CreateRenderer(
        bool useNativeWindow,
        IWindowService windowService,
        EngineRuntimeInfo runtimeInfo,
        ContractsProvider renderInputProvider)
    {
        return useNativeWindow
            ? new NullRenderer(windowService, runtimeInfo, renderInputProvider)
            : new HeadlessRenderer();
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

    public void InitializeScene()
    {
        mSceneGraphService.AddRootNode();
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
    private readonly IWindowService mWindowService;
    private readonly IRenderer mRenderer;
    private readonly ISceneRuntime mSceneRuntime;
    private readonly IAssetService mAssetService;
    private readonly IInputService mInputService;
    private readonly ITimeService mTimeService;
    private readonly double? mAutoExitSeconds;

    public ApplicationHost(
        IWindowService windowService,
        IRenderer renderer,
        ISceneRuntime sceneRuntime,
        IAssetService assetService,
        IInputService inputService,
        ITimeService timeService)
    {
        mWindowService = windowService;
        mRenderer = renderer;
        mSceneRuntime = sceneRuntime;
        mAssetService = assetService;
        mInputService = inputService;
        mTimeService = timeService;
        mAutoExitSeconds = ResolveAutoExitSeconds();
    }

    public int Run()
    {
        try
        {
            mRenderer.Initialize();
            var uptime = Stopwatch.StartNew();
            mSceneRuntime.InitializeScene();
            _ = mAssetService.Load("bootstrap://placeholder");

            while (mWindowService.Exists && !mWindowService.IsCloseRequested)
            {
                mWindowService.ProcessEvents();
                _ = mInputService.GetSnapshot();
                _ = mTimeService.Current;
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
