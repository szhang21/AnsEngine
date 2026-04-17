using Engine.App;
using Engine.Asset;
using Engine.Contracts;
using Engine.Core;
using Engine.Platform;
using Engine.Render;
using Engine.Scene;
using System.Reflection;
using System.Numerics;
using Xunit;
using ContractsProvider = Engine.Contracts.ISceneRenderContractProvider;

namespace Engine.App.Tests;

public sealed class RuntimeBootstrapTests
{
    [Fact]
    public void CreateRenderer_NativePath_UsesContractsProvider()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var sceneGraph = new SceneGraphService(runtimeInfo);
        ContractsProvider provider = sceneGraph;
        var windowService = new TestWindowService();
        var method = typeof(RuntimeBootstrap).GetMethod("CreateRenderer", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var renderer = Assert.IsType<NullRenderer>(method!.Invoke(null, new object[] { true, windowService, runtimeInfo, provider }));
        var providerField = typeof(NullRenderer).GetField("mSceneProvider", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(providerField);
        var wiredProvider = Assert.IsAssignableFrom<ContractsProvider>(providerField!.GetValue(renderer));

        Assert.Same(provider, wiredProvider);

        sceneGraph.AddRootNode();
        var firstFrame = wiredProvider.BuildRenderFrame();
        var secondFrame = wiredProvider.BuildRenderFrame();
        var firstItem = Assert.Single(firstFrame.Items);
        var secondItem = Assert.Single(secondFrame.Items);

        Assert.Equal(Vector3.Zero, firstItem.Transform.Position);
        Assert.Equal(Quaternion.Identity, firstItem.Transform.Rotation);
        Assert.NotEqual(firstItem.Transform, secondItem.Transform);
        Assert.NotEqual(Matrix4x4.Identity, firstFrame.Camera.View);
        Assert.NotEqual(Matrix4x4.Identity, firstFrame.Camera.Projection);
        Assert.NotEqual(firstFrame.Camera.View, secondFrame.Camera.View);
        Assert.Equal(firstFrame.Camera.Projection, secondFrame.Camera.Projection);
    }

    [Fact]
    public void ApplicationHost_Run_InitializesSceneRuntimeThroughAbstraction()
    {
        var windowService = new AutoCloseWindowService();
        var renderer = new CountingRenderer();
        var sceneRuntime = new SpySceneRuntime();
        var app = new ApplicationHost(
            windowService,
            renderer,
            sceneRuntime,
            new NullAssetService(new EngineRuntimeInfo("AnsEngine", "0.1.0"), windowService),
            new NullInputService(),
            new FixedTimeService(new TimeSnapshot(0.016, 0, 60)));

        var exitCode = app.Run();

        Assert.Equal(0, exitCode);
        Assert.True(sceneRuntime.InitializeCalled);
        Assert.Equal(1, renderer.InitializeCalls);
        Assert.True(renderer.RenderCalls >= 1);
        Assert.Equal(1, renderer.ShutdownCalls);
    }

    [Fact]
    public void ApplicationHost_Run_RenderFrameThrows_RequestsCloseAndShutsDown()
    {
        var windowService = new TrackingWindowService();
        var renderer = new FailingRenderer();
        var sceneRuntime = new SpySceneRuntime();
        var app = new ApplicationHost(
            windowService,
            renderer,
            sceneRuntime,
            new NullAssetService(new EngineRuntimeInfo("AnsEngine", "0.1.0"), windowService),
            new NullInputService(),
            new FixedTimeService(new TimeSnapshot(0.016, 0, 60)));

        var exitCode = app.Run();

        Assert.Equal(1, exitCode);
        Assert.True(sceneRuntime.InitializeCalled);
        Assert.Equal(1, renderer.InitializeCalls);
        Assert.Equal(1, renderer.RenderCalls);
        Assert.Equal(1, renderer.ShutdownCalls);
        Assert.Equal(1, windowService.RequestCloseCalls);
        Assert.Equal(1, windowService.DisposeCalls);
        Assert.True(windowService.IsCloseRequested);
    }

    private sealed class TestWindowService : IWindowService
    {
        public WindowConfig Configuration { get; } = new(1280, 720, "AnsEngine-Tests");
        public bool Exists => true;
        public bool IsCloseRequested => false;
        public void ProcessEvents(double timeoutSeconds = 0) { }
        public void Present() { }
        public void RequestClose() { }
        public void Dispose() { }
    }

    private sealed class AutoCloseWindowService : IWindowService
    {
        public WindowConfig Configuration { get; } = new(1280, 720, "AnsEngine-Tests");
        public bool Exists => true;
        public bool IsCloseRequested { get; private set; }
        public void ProcessEvents(double timeoutSeconds = 0) { }
        public void Present()
        {
            IsCloseRequested = true;
        }

        public void RequestClose()
        {
            IsCloseRequested = true;
        }

        public void Dispose() { }
    }

    private sealed class CountingRenderer : IRenderer
    {
        public int InitializeCalls { get; private set; }
        public int RenderCalls { get; private set; }
        public int ShutdownCalls { get; private set; }

        public void Initialize()
        {
            InitializeCalls += 1;
        }

        public void RenderFrame()
        {
            RenderCalls += 1;
        }

        public void Shutdown()
        {
            ShutdownCalls += 1;
        }
    }

    private sealed class FailingRenderer : IRenderer
    {
        public int InitializeCalls { get; private set; }
        public int RenderCalls { get; private set; }
        public int ShutdownCalls { get; private set; }

        public void Initialize()
        {
            InitializeCalls += 1;
        }

        public void RenderFrame()
        {
            RenderCalls += 1;
            throw new InvalidOperationException("Synthetic render failure.");
        }

        public void Shutdown()
        {
            ShutdownCalls += 1;
        }
    }

    private sealed class TrackingWindowService : IWindowService
    {
        public WindowConfig Configuration { get; } = new(1280, 720, "AnsEngine-Tests");
        public bool Exists => true;
        public bool IsCloseRequested { get; private set; }
        public int RequestCloseCalls { get; private set; }
        public int DisposeCalls { get; private set; }

        public void ProcessEvents(double timeoutSeconds = 0)
        {
        }

        public void Present()
        {
        }

        public void RequestClose()
        {
            RequestCloseCalls += 1;
            IsCloseRequested = true;
        }

        public void Dispose()
        {
            DisposeCalls += 1;
        }
    }

    private sealed class SpySceneRuntime : ISceneRuntime
    {
        public bool InitializeCalled { get; private set; }

        public void InitializeScene()
        {
            InitializeCalled = true;
        }
    }
}
