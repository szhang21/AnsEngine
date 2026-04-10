using Engine.Asset;
using Engine.Core;
using Engine.Platform;
using Engine.Render;
using Engine.Scene;
using System.Diagnostics;

namespace Engine.App;

public sealed class RuntimeBootstrap : IRuntimeBootstrap
{
    public IApplication Build()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var windowService = new NullWindowService(new WindowConfig(1280, 720, "AnsEngine"));
        var inputService = new NullInputService();
        var timeService = new FixedTimeService(new TimeSnapshot(0.016, 0, 60));
        var renderer = new NullRenderer(windowService, runtimeInfo);
        var sceneGraph = new SceneGraphService(runtimeInfo);
        var assetService = new NullAssetService(runtimeInfo, windowService);
        return new ApplicationHost(windowService, renderer, sceneGraph, assetService, inputService, timeService);
    }
}

public sealed class ApplicationHost : IApplication
{
    private readonly IWindowService _windowService;
    private readonly IRenderer _renderer;
    private readonly SceneGraphService _sceneGraph;
    private readonly IAssetService _assetService;
    private readonly IInputService _inputService;
    private readonly ITimeService _timeService;
    private readonly double? _autoExitSeconds;

    public ApplicationHost(
        IWindowService windowService,
        IRenderer renderer,
        SceneGraphService sceneGraph,
        IAssetService assetService,
        IInputService inputService,
        ITimeService timeService)
    {
        _windowService = windowService;
        _renderer = renderer;
        _sceneGraph = sceneGraph;
        _assetService = assetService;
        _inputService = inputService;
        _timeService = timeService;
        _autoExitSeconds = ResolveAutoExitSeconds();
    }

    public int Run()
    {
        _renderer.Initialize();
        var uptime = Stopwatch.StartNew();

        try
        {
            _sceneGraph.AddRootNode();
            _ = _assetService.Load("bootstrap://placeholder");

            while (_windowService.Exists && !_windowService.IsCloseRequested)
            {
                _windowService.ProcessEvents();
                _ = _inputService.GetSnapshot();
                _ = _timeService.Current;
                _renderer.RenderFrame();
                _windowService.Present();

                if (_autoExitSeconds.HasValue && uptime.Elapsed.TotalSeconds >= _autoExitSeconds.Value)
                {
                    _windowService.RequestClose();
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex}");
            return 1;
        }
        finally
        {
            _renderer.Shutdown();
            _windowService.Dispose();
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
