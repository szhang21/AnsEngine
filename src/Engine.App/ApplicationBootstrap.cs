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
    private readonly SceneGraphService sceneGraphService;

    public SceneRuntimeAdapter(SceneGraphService sceneGraphService)
    {
        this.sceneGraphService = sceneGraphService ?? throw new ArgumentNullException(nameof(sceneGraphService));
    }

    public void InitializeScene()
    {
        sceneGraphService.AddRootNode();
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
    private readonly IWindowService windowService;
    private readonly IRenderer renderer;
    private readonly ISceneRuntime sceneRuntime;
    private readonly IAssetService assetService;
    private readonly IInputService inputService;
    private readonly ITimeService timeService;
    private readonly double? autoExitSeconds;

    public ApplicationHost(
        IWindowService windowService,
        IRenderer renderer,
        ISceneRuntime sceneRuntime,
        IAssetService assetService,
        IInputService inputService,
        ITimeService timeService)
    {
        this.windowService = windowService;
        this.renderer = renderer;
        this.sceneRuntime = sceneRuntime;
        this.assetService = assetService;
        this.inputService = inputService;
        this.timeService = timeService;
        autoExitSeconds = ResolveAutoExitSeconds();
    }

    public int Run()
    {
        try
        {
            renderer.Initialize();
            var uptime = Stopwatch.StartNew();
            sceneRuntime.InitializeScene();
            _ = assetService.Load("bootstrap://placeholder");

            while (windowService.Exists && !windowService.IsCloseRequested)
            {
                windowService.ProcessEvents();
                _ = inputService.GetSnapshot();
                _ = timeService.Current;
                renderer.RenderFrame();
                windowService.Present();

                if (autoExitSeconds.HasValue && uptime.Elapsed.TotalSeconds >= autoExitSeconds.Value)
                {
                    windowService.RequestClose();
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            if (windowService.Exists && !windowService.IsCloseRequested)
            {
                windowService.RequestClose();
            }

            Console.Error.WriteLine($"Fatal error: {ex}");
            return 1;
        }
        finally
        {
            renderer.Shutdown();
            windowService.Dispose();
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
