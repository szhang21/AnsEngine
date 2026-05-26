using Engine.App;
using Engine.Asset;
using Engine.Contracts;
using Engine.Core;
using Engine.Platform;
using Engine.Render;
using Engine.Runtime;
using Engine.Runtime.Abstractions;
using Engine.Scene;
using Engine.SceneData;
using Engine.SceneData.Abstractions;
using System.Numerics;
using System.Reflection;
using Xunit;
using ContractsProvider = Engine.Contracts.ISceneRenderContractProvider;

namespace Engine.App.Tests;

public sealed class RuntimeBootstrapTests
{
    [Fact]
    public void CreateRenderer_NativePath_UsesContractsProvider()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");
        var provider = new StubRenderProvider();
        var windowService = new TestWindowService();
        var method = typeof(RuntimeBootstrap).GetMethod("CreateRenderer", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        IMeshAssetProvider meshAssetProvider = new StubMeshAssetProvider();

        var renderer = Assert.IsType<NullRenderer>(
            method!.Invoke(null, new object[] { true, windowService, runtimeInfo, provider, meshAssetProvider }));
        var providerField = typeof(NullRenderer).GetField("mSceneProvider", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(providerField);
        var wiredProvider = Assert.IsAssignableFrom<ContractsProvider>(providerField!.GetValue(renderer));
        var meshProviderField = typeof(NullRenderer).GetField("mMeshAssetProvider", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(meshProviderField);
        var wiredMeshProvider = Assert.IsAssignableFrom<IMeshAssetProvider>(meshProviderField!.GetValue(renderer));

        Assert.Same(provider, wiredProvider);
        Assert.Same(meshAssetProvider, wiredMeshProvider);
    }

    [Fact]
    public void ApplicationHost_Run_InitializesAndTicksRuntimeSessionThroughHostContract()
    {
        var runtimeSession = new SpyRuntimeSession();
        var renderer = new CountingRenderer();
        var app = CreateApp(runtimeSession, renderer: renderer);

        var exitCode = app.Run();

        Assert.Equal(0, exitCode);
        Assert.Equal(1, runtimeSession.InitializeCalls);
        Assert.Equal(1, runtimeSession.TickCalls);
        Assert.Equal(1, renderer.InitializeCalls);
        Assert.Equal(1, renderer.RenderCalls);
        Assert.Equal(1, renderer.ShutdownCalls);
    }

    [Fact]
    public void ApplicationHost_Run_OrdersRuntimeTickBeforeRenderAndPresent()
    {
        var callLog = new List<string>();
        var runtimeSession = new SpyRuntimeSession(callLog);
        var app = CreateApp(
            runtimeSession,
            windowService: new AutoCloseWindowService(callLog),
            renderer: new CountingRenderer(callLog),
            inputService: new StubInputService(InputSnapshot.FromKeys(EngineKey.D), callLog),
            timeService: new StubTimeService(new TimeSnapshot(0.25, 1.25, 4.0), callLog));

        var exitCode = app.Run();

        Assert.Equal(0, exitCode);
        Assert.True(callLog.IndexOf("ProcessEvents") < callLog.IndexOf("Input"));
        Assert.True(callLog.IndexOf("Input") < callLog.IndexOf("Time"));
        Assert.True(callLog.IndexOf("Time") < callLog.IndexOf("RuntimeTick"));
        Assert.True(callLog.IndexOf("RuntimeTick") < callLog.IndexOf("RenderFrame"));
        Assert.True(callLog.IndexOf("RenderFrame") < callLog.IndexOf("Present"));
    }

    [Fact]
    public void ApplicationHost_Run_ConvertsPlatformInputToRuntimeInputBeforeTick()
    {
        var runtimeSession = new SpyRuntimeSession();
        var app = CreateApp(
            runtimeSession,
            inputService: new StubInputService(InputSnapshot.FromKeys(EngineKey.W, EngineKey.A)));

        var exitCode = app.Run();

        Assert.Equal(0, exitCode);
        Assert.True(runtimeSession.LastTickContext.Input.IsKeyDown(RuntimeKey.W));
        Assert.True(runtimeSession.LastTickContext.Input.IsKeyDown(RuntimeKey.A));
        Assert.False(runtimeSession.LastTickContext.Input.IsKeyDown(RuntimeKey.S));
        Assert.False(runtimeSession.LastTickContext.Input.IsKeyDown(RuntimeKey.D));
    }

    [Fact]
    public void ApplicationHost_Run_RuntimeInitializeFailureReturnsErrorBeforeTickOrRender()
    {
        var runtimeSession = new SpyRuntimeSession
        {
            InitializeResult = RuntimeInitializationResult.FailureResult(
                new RuntimeFailure("Initialize", "Synthetic initialize failure."))
        };
        var renderer = new CountingRenderer();
        var app = CreateApp(runtimeSession, renderer: renderer);

        var exitCode = app.Run();

        Assert.Equal(1, exitCode);
        Assert.Equal(1, runtimeSession.InitializeCalls);
        Assert.Equal(0, runtimeSession.TickCalls);
        Assert.Equal(0, renderer.RenderCalls);
        Assert.Equal(1, renderer.ShutdownCalls);
    }

    [Fact]
    public void ApplicationHost_Run_RuntimeTickFailureReturnsErrorBeforeRender()
    {
        var runtimeSession = new SpyRuntimeSession
        {
            TickResult = RuntimeTickResult.FailureResult(
                new RuntimeFailure("ScriptUpdate", "Synthetic tick failure.", "cube-main", "FailingScript"))
        };
        var renderer = new CountingRenderer();
        var app = CreateApp(runtimeSession, renderer: renderer);

        var exitCode = app.Run();

        Assert.Equal(1, exitCode);
        Assert.Equal(1, runtimeSession.InitializeCalls);
        Assert.Equal(1, runtimeSession.TickCalls);
        Assert.Equal(0, renderer.RenderCalls);
        Assert.Equal(1, renderer.ShutdownCalls);
    }

    [Fact]
    public void ApplicationHost_Run_LoaderFailureSkipsRuntimeInitialization()
    {
        var runtimeSession = new SpyRuntimeSession();
        var renderer = new CountingRenderer();
        var app = CreateApp(
            runtimeSession,
            renderer: renderer,
            sceneDescriptionLoader: new FailingSceneDescriptionLoader());

        var exitCode = app.Run();

        Assert.Equal(1, exitCode);
        Assert.Equal(0, runtimeSession.InitializeCalls);
        Assert.Equal(0, runtimeSession.TickCalls);
        Assert.Equal(0, renderer.RenderCalls);
        Assert.Equal(1, renderer.ShutdownCalls);
    }

    [Fact]
    public void ApplicationHost_Run_RenderFrameThrows_RequestsCloseAndShutsDown()
    {
        var windowService = new TrackingWindowService();
        var runtimeSession = new SpyRuntimeSession();
        var renderer = new FailingRenderer();
        var app = CreateApp(runtimeSession, windowService: windowService, renderer: renderer);

        var exitCode = app.Run();

        Assert.Equal(1, exitCode);
        Assert.Equal(1, runtimeSession.InitializeCalls);
        Assert.Equal(1, runtimeSession.TickCalls);
        Assert.Equal(1, renderer.InitializeCalls);
        Assert.Equal(1, renderer.RenderCalls);
        Assert.Equal(1, renderer.ShutdownCalls);
        Assert.Equal(1, windowService.RequestCloseCalls);
        Assert.Equal(1, windowService.DisposeCalls);
    }

    [Fact]
    public void ApplicationHost_Run_RealRuntimeSessionMoveOnInputSceneUpdatesBeforeRender()
    {
        var runtimeSession = new EngineRuntimeSessionHost(new EngineRuntimeSession());
        var renderer = new CapturingRenderer(runtimeSession.SceneRenderProvider);
        var app = CreateApp(
            runtimeSession,
            renderer: renderer,
            sceneDescriptionLoader: new MoveOnInputSceneDescriptionLoader(),
            inputService: new StubInputService(InputSnapshot.FromKeys(EngineKey.D)),
            timeService: new FixedTimeService(new TimeSnapshot(0.5, 0.5, 2.0)));

        var exitCode = app.Run();

        Assert.Equal(0, exitCode);
        Assert.Equal(1, renderer.RenderCalls);
        AssertVectorNearlyEqual(new Vector3(1.0f, 0.0f, 0.0f), renderer.FirstRenderedPosition!.Value);
    }

    [Fact]
    public void ApplicationHost_DoesNotKeepDirectScriptOrPhysicsOrchestrationFields()
    {
        var fields = typeof(ApplicationHost).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.DoesNotContain(fields, field => field.FieldType.FullName?.Contains("Engine.Scripting", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(fields, field => field.FieldType.FullName?.Contains("Engine.Physics", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(fields, field => field.FieldType.Name.Contains("RuntimePhysicsOrchestrator", StringComparison.Ordinal));
        Assert.Contains(fields, field => field.FieldType == typeof(IRuntimeSessionHost));
        Assert.Null(typeof(RuntimeBootstrap).Assembly.GetType("Engine.App.SceneRuntimeAdapter"));
        Assert.Null(typeof(RuntimeBootstrap).Assembly.GetType("Engine.App.RuntimePhysicsOrchestrator"));
        Assert.Null(typeof(RuntimeBootstrap).Assembly.GetType("Engine.App.RotateSelfScript"));
        Assert.Null(typeof(RuntimeBootstrap).Assembly.GetType("Engine.App.MoveOnInputScript"));
    }

    [Fact]
    public void JsonSceneDescriptionLoader_BundledDefaultSceneInitializesRuntimeSession()
    {
        var scenePath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "..",
                "src",
                "Engine.App",
                "SampleScenes",
                "default.scene.json"));
        var loadResult = new JsonSceneDescriptionLoader().Load(scenePath);
        Assert.True(loadResult.IsSuccess, loadResult.Failure?.Message);
        var runtimeSession = new EngineRuntimeSessionHost(new EngineRuntimeSession());

        var initializeResult = runtimeSession.Initialize(loadResult.Scene!);

        Assert.True(initializeResult.IsSuccess, initializeResult.Failure?.Message);
    }

    [Fact]
    public void ResolveSceneFilePath_UsesEnvironmentOverrideWhenPresent()
    {
        const string expectedPath = "/tmp/custom.scene.json";
        var method = typeof(RuntimeBootstrap).GetMethod("ResolveSceneFilePath", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var previousValue = Environment.GetEnvironmentVariable("ANS_ENGINE_SCENE_PATH");
        try
        {
            Environment.SetEnvironmentVariable("ANS_ENGINE_SCENE_PATH", expectedPath);

            var resolvedPath = Assert.IsType<string>(method!.Invoke(null, Array.Empty<object>()));

            Assert.Equal(expectedPath, resolvedPath);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ANS_ENGINE_SCENE_PATH", previousValue);
        }
    }

    [Fact]
    public void ResolveSceneFilePath_UsesBundledSampleSceneByDefault()
    {
        var method = typeof(RuntimeBootstrap).GetMethod("ResolveSceneFilePath", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var previousValue = Environment.GetEnvironmentVariable("ANS_ENGINE_SCENE_PATH");
        try
        {
            Environment.SetEnvironmentVariable("ANS_ENGINE_SCENE_PATH", null);

            var resolvedPath = Assert.IsType<string>(method!.Invoke(null, Array.Empty<object>()));

            Assert.EndsWith(Path.Combine("SampleScenes", "default.scene.json"), resolvedPath, StringComparison.Ordinal);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ANS_ENGINE_SCENE_PATH", previousValue);
        }
    }

    [Fact]
    public void CreateInputService_NativePath_UsesNativeWindowInputService()
    {
        var method = typeof(RuntimeBootstrap).GetMethod("CreateInputService", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var inputService = Assert.IsType<NativeWindowInputService>(
            method!.Invoke(null, new object[] { true, new TestKeyboardStateProvider(EngineKey.A) }));

        var snapshot = inputService.GetSnapshot();

        Assert.True(snapshot.IsKeyDown(EngineKey.A));
        Assert.False(snapshot.IsKeyDown(EngineKey.W));
    }

    [Fact]
    public void CreateInputService_HeadlessPath_UsesNullInputService()
    {
        var method = typeof(RuntimeBootstrap).GetMethod("CreateInputService", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var inputService = Assert.IsType<NullInputService>(
            method!.Invoke(null, new object[] { false, new TestKeyboardStateProvider(EngineKey.W) }));

        var snapshot = inputService.GetSnapshot();

        Assert.Equal(InputSnapshot.Empty, snapshot);
    }

    private static ApplicationHost CreateApp(
        IRuntimeSessionHost runtimeSession,
        IWindowService? windowService = null,
        IRenderer? renderer = null,
        ISceneDescriptionLoader? sceneDescriptionLoader = null,
        IInputService? inputService = null,
        ITimeService? timeService = null)
    {
        var resolvedWindowService = windowService ?? new AutoCloseWindowService();
        return new ApplicationHost(
            resolvedWindowService,
            renderer ?? new CountingRenderer(),
            runtimeSession,
            new NullAssetService(new EngineRuntimeInfo("AnsEngine", "0.1.0"), resolvedWindowService),
            new StubMeshAssetProvider(),
            sceneDescriptionLoader ?? new SuccessfulSceneDescriptionLoader(),
            "sample.scene.json",
            inputService ?? new NullInputService(),
            timeService ?? new FixedTimeService(new TimeSnapshot(0.016, 0.016, 60)));
    }

    private static RuntimeSceneSnapshot CreateEmptyRuntimeSnapshot()
    {
        return new RuntimeSceneSnapshot(
            Array.Empty<SceneRuntimeObjectSnapshot>(),
            new SceneCameraRuntimeSnapshot(Vector3.UnitZ, Vector3.Zero, 1.0f),
            0,
            0.0d);
    }

    private static void AssertVectorNearlyEqual(Vector3 expected, Vector3 actual)
    {
        Assert.Equal(expected.X, actual.X, 5);
        Assert.Equal(expected.Y, actual.Y, 5);
        Assert.Equal(expected.Z, actual.Z, 5);
    }

    private sealed class SpyRuntimeSession : IRuntimeSessionHost
    {
        private readonly List<string>? mCallLog;

        public SpyRuntimeSession(List<string>? callLog = null)
        {
            mCallLog = callLog;
            InitializeResult = RuntimeInitializationResult.Success();
            TickResult = RuntimeTickResult.Success(CreateEmptyRuntimeSnapshot());
        }

        public int InitializeCalls { get; private set; }

        public int TickCalls { get; private set; }

        public RuntimeTickContext LastTickContext { get; private set; } =
            new(0.0d, 0.0d, RuntimeInputSnapshot.Empty);

        public RuntimeInitializationResult InitializeResult { get; set; }

        public RuntimeTickResult TickResult { get; set; }

        public RuntimeInitializationResult Initialize(SceneDescription sceneDescription)
        {
            InitializeCalls += 1;
            return InitializeResult;
        }

        public RuntimeTickResult Tick(RuntimeTickContext context)
        {
            TickCalls += 1;
            LastTickContext = context;
            mCallLog?.Add("RuntimeTick");
            return TickResult;
        }
    }

    private sealed class StubRenderProvider : ContractsProvider
    {
        public SceneRenderFrame BuildRenderFrame()
        {
            return new SceneRenderFrame(
                0,
                Array.Empty<SceneRenderItem>(),
                new SceneCamera(Matrix4x4.Identity, Matrix4x4.Identity));
        }
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

    private sealed class TestKeyboardStateProvider : IKeyboardStateProvider
    {
        private readonly HashSet<EngineKey> mPressedKeys;

        public TestKeyboardStateProvider(params EngineKey[] pressedKeys)
        {
            mPressedKeys = new HashSet<EngineKey>(pressedKeys);
        }

        public bool IsKeyDown(EngineKey key)
        {
            return mPressedKeys.Contains(key);
        }
    }

    private sealed class AutoCloseWindowService : IWindowService
    {
        private readonly List<string>? mCallLog;

        public AutoCloseWindowService(List<string>? callLog = null)
        {
            mCallLog = callLog;
        }

        public WindowConfig Configuration { get; } = new(1280, 720, "AnsEngine-Tests");
        public bool Exists => true;
        public bool IsCloseRequested { get; private set; }

        public void ProcessEvents(double timeoutSeconds = 0)
        {
            mCallLog?.Add("ProcessEvents");
        }

        public void Present()
        {
            mCallLog?.Add("Present");
            IsCloseRequested = true;
        }

        public void RequestClose()
        {
            IsCloseRequested = true;
        }

        public void Dispose()
        {
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

    private sealed class CountingRenderer : IRenderer
    {
        private readonly List<string>? mCallLog;

        public CountingRenderer(List<string>? callLog = null)
        {
            mCallLog = callLog;
        }

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
            mCallLog?.Add("RenderFrame");
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

    private sealed class CapturingRenderer : IRenderer
    {
        private readonly ContractsProvider mProvider;

        public CapturingRenderer(ContractsProvider provider)
        {
            mProvider = provider;
        }

        public int RenderCalls { get; private set; }

        public Vector3? FirstRenderedPosition { get; private set; }

        public void Initialize()
        {
        }

        public void RenderFrame()
        {
            RenderCalls += 1;
            var transform = Assert.Single(mProvider.BuildRenderFrame().Items).Transform;
            FirstRenderedPosition ??= transform.Position;
        }

        public void Shutdown()
        {
        }
    }

    private sealed class StubInputService : IInputService
    {
        private readonly InputSnapshot mSnapshot;
        private readonly List<string>? mCallLog;

        public StubInputService(InputSnapshot snapshot, List<string>? callLog = null)
        {
            mSnapshot = snapshot;
            mCallLog = callLog;
        }

        public InputSnapshot GetSnapshot()
        {
            mCallLog?.Add("Input");
            return mSnapshot;
        }
    }

    private sealed class StubTimeService : ITimeService
    {
        private readonly TimeSnapshot mSnapshot;
        private readonly List<string> mCallLog;

        public StubTimeService(TimeSnapshot snapshot, List<string> callLog)
        {
            mSnapshot = snapshot;
            mCallLog = callLog;
        }

        public TimeSnapshot Current
        {
            get
            {
                mCallLog.Add("Time");
                return mSnapshot;
            }
        }
    }

    private sealed class StubMeshAssetProvider : IMeshAssetProvider
    {
        private readonly MeshAssetData mAsset = new(
            new[]
            {
                new MeshAssetVertex(Vector3.Zero, Vector3.UnitZ, Vector2.Zero),
                new MeshAssetVertex(Vector3.UnitX, Vector3.UnitZ, Vector2.UnitX),
                new MeshAssetVertex(Vector3.UnitY, Vector3.UnitZ, Vector2.UnitY)
            },
            new[] { 0, 1, 2 });

        public MeshAssetLoadResult GetMesh(SceneMeshRef mesh)
        {
            return MeshAssetLoadResult.Success(mesh, mAsset);
        }
    }

    private sealed class SuccessfulSceneDescriptionLoader : ISceneDescriptionLoader
    {
        public SceneDescriptionLoadResult Load(string sceneFilePath)
        {
            return SceneDescriptionLoadResult.Success(
                new SceneDescription(
                    "sample-scene",
                    sceneFilePath,
                    new SceneCameraDescription(new Vector3(0.0f, 0.25f, 2.2f), Vector3.Zero, 1.0471976f),
                    new[]
                    {
                        new SceneObjectDescription(
                            "cube-main",
                            "Cube Main",
                            new SceneMeshRef("mesh://cube"),
                            new SceneMaterialRef("material://default"),
                            SceneTransformDescription.Identity)
                    }));
        }
    }

    private sealed class MoveOnInputSceneDescriptionLoader : ISceneDescriptionLoader
    {
        public SceneDescriptionLoadResult Load(string sceneFilePath)
        {
            return SceneDescriptionLoadResult.Success(
                new SceneDescription(
                    "script-scene",
                    sceneFilePath,
                    new SceneCameraDescription(new Vector3(0.0f, 0.25f, 2.2f), Vector3.Zero, 1.0471976f),
                    new[]
                    {
                        new SceneObjectDescription(
                            "cube-main",
                            "Cube Main",
                            new SceneComponentDescription[]
                            {
                                new SceneTransformComponentDescription(SceneTransformDescription.Identity),
                                new SceneMeshRendererComponentDescription(
                                    new SceneMeshRef("mesh://cube"),
                                    new SceneMaterialRef("material://default")),
                                new SceneScriptComponentDescription(
                                    "MoveOnInput",
                                    new Dictionary<string, SceneScriptPropertyValue>
                                    {
                                        ["speedUnitsPerSecond"] = SceneScriptPropertyValue.FromNumber(2.0d)
                                    })
                            })
                    }));
        }
    }

    private sealed class FailingSceneDescriptionLoader : ISceneDescriptionLoader
    {
        public SceneDescriptionLoadResult Load(string sceneFilePath)
        {
            return SceneDescriptionLoadResult.FailureResult(
                new SceneDescriptionLoadFailure(
                    SceneDescriptionLoadFailureKind.NotFound,
                    "Scene file was not found.",
                    sceneFilePath));
        }
    }
}
