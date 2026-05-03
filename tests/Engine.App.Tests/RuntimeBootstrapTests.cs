using Engine.App;
using Engine.Asset;
using Engine.Contracts;
using Engine.Core;
using Engine.Platform;
using Engine.Render;
using Engine.Scene;
using Engine.SceneData;
using Engine.SceneData.Abstractions;
using Engine.Scripting;
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
        IMeshAssetProvider meshAssetProvider = new StubMeshAssetProvider();

        var renderer = Assert.IsType<NullRenderer>(method!.Invoke(null, new object[] { true, windowService, runtimeInfo, provider, meshAssetProvider }));
        var providerField = typeof(NullRenderer).GetField("mSceneProvider", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(providerField);
        var wiredProvider = Assert.IsAssignableFrom<ContractsProvider>(providerField!.GetValue(renderer));
        var meshProviderField = typeof(NullRenderer).GetField("mMeshAssetProvider", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(meshProviderField);
        var wiredMeshProvider = Assert.IsAssignableFrom<IMeshAssetProvider>(meshProviderField!.GetValue(renderer));

        Assert.Same(provider, wiredProvider);
        Assert.Same(meshAssetProvider, wiredMeshProvider);

        sceneGraph.AddRootNode();
        var firstFrame = wiredProvider.BuildRenderFrame();
        var secondFrame = wiredProvider.BuildRenderFrame();
        var firstItem = Assert.Single(firstFrame.Items);
        var secondItem = Assert.Single(secondFrame.Items);

        Assert.Equal(Vector3.Zero, firstItem.Transform.Position);
        Assert.Equal(Quaternion.Identity, firstItem.Transform.Rotation);
        Assert.Equal(firstItem.Transform, secondItem.Transform);
        Assert.NotEqual(Matrix4x4.Identity, firstFrame.Camera.View);
        Assert.NotEqual(Matrix4x4.Identity, firstFrame.Camera.Projection);
        Assert.Equal(firstFrame.Camera.View, secondFrame.Camera.View);
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
            new StubMeshAssetProvider(),
            new SuccessfulSceneDescriptionLoader(),
            "sample.scene.json",
            new NullInputService(),
            new FixedTimeService(new TimeSnapshot(0.016, 0, 60)));

        var exitCode = app.Run();

        Assert.Equal(0, exitCode);
        Assert.True(sceneRuntime.InitializeCalled);
        Assert.Equal(1, sceneRuntime.UpdateCalls);
        Assert.Equal(1, renderer.InitializeCalls);
        Assert.True(renderer.RenderCalls >= 1);
        Assert.Equal(1, renderer.ShutdownCalls);
    }

    [Fact]
    public void ApplicationHost_Run_UpdatesSceneRuntimeBeforeRender()
    {
        var callLog = new List<string>();
        var windowService = new AutoCloseWindowService(callLog);
        var renderer = new CountingRenderer(callLog);
        var sceneRuntime = new SpySceneRuntime(callLog);
        var inputService = new StubInputService(new InputSnapshot(true), callLog);
        var timeService = new StubTimeService(new TimeSnapshot(0.25, 1.5, 4.0), callLog);
        var app = new ApplicationHost(
            windowService,
            renderer,
            sceneRuntime,
            new NullAssetService(new EngineRuntimeInfo("AnsEngine", "0.1.0"), windowService),
            new StubMeshAssetProvider(),
            new SuccessfulSceneDescriptionLoader(),
            "sample.scene.json",
            inputService,
            timeService);

        var exitCode = app.Run();

        Assert.Equal(0, exitCode);
        Assert.Equal(1, sceneRuntime.UpdateCalls);
        Assert.Equal(new TimeSnapshot(0.25, 1.5, 4.0), sceneRuntime.LastTime);
        Assert.Equal(new InputSnapshot(true), sceneRuntime.LastInput);
        Assert.True(callLog.IndexOf("ProcessEvents") < callLog.IndexOf("Input"));
        Assert.True(callLog.IndexOf("Input") < callLog.IndexOf("Time"));
        Assert.True(callLog.IndexOf("Time") < callLog.IndexOf("SceneUpdate"));
        Assert.True(callLog.IndexOf("SceneUpdate") < callLog.IndexOf("RenderFrame"));
        Assert.True(callLog.IndexOf("RenderFrame") < callLog.IndexOf("Present"));
    }

    [Fact]
    public void SceneRuntimeAdapter_Update_TranslatesTimeAndInputToSceneUpdateContext()
    {
        var sceneGraph = new SceneGraphService(new EngineRuntimeInfo("AnsEngine", "0.1.0"));
        sceneGraph.AddRootNode();
        var sceneRuntime = CreateSceneRuntime(sceneGraph);

        sceneRuntime.Update(new TimeSnapshot(0.25, 1.5, 4.0), new InputSnapshot(true));

        var snapshot = sceneGraph.CreateRuntimeSnapshot();
        var renderItem = Assert.Single(sceneGraph.BuildRenderFrame().Items);
        Assert.Equal(1, snapshot.UpdateFrameCount);
        Assert.Equal(0.25d, snapshot.AccumulatedUpdateSeconds);
        AssertQuaternionNearlyEqual(Quaternion.Identity, renderItem.Transform.Rotation);
    }

    [Fact]
    public void ApplicationHost_Run_RotateSelfSceneUpdatesScriptBeforeRender()
    {
        var sceneGraph = new SceneGraphService(new EngineRuntimeInfo("AnsEngine", "0.1.0"));
        var sceneRuntime = CreateSceneRuntime(sceneGraph);
        var renderer = new CapturingRenderer(sceneGraph);
        var windowService = new AutoCloseWindowService();
        var scriptRuntime = CreateRotateSelfRuntime();
        var app = new ApplicationHost(
            windowService,
            renderer,
            sceneRuntime,
            new NullAssetService(new EngineRuntimeInfo("AnsEngine", "0.1.0"), windowService),
            new StubMeshAssetProvider(),
            new ScriptSceneDescriptionLoader("RotateSelf"),
            "script.scene.json",
            new NullInputService(),
            new FixedTimeService(new TimeSnapshot(0.5, 0.5, 2.0)),
            scriptRuntime);

        var exitCode = app.Run();

        Assert.Equal(0, exitCode);
        Assert.Equal(1, renderer.RenderCalls);
        Assert.NotNull(renderer.FirstRenderedRotation);
        AssertQuaternionNearlyEqual(
            Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.5f),
            renderer.FirstRenderedRotation!.Value);
    }

    [Fact]
    public void ApplicationHost_Run_MoveOnInputNoInputDoesNotMoveBeforeRender()
    {
        var sceneGraph = new SceneGraphService(new EngineRuntimeInfo("AnsEngine", "0.1.0"));
        var sceneRuntime = CreateSceneRuntime(sceneGraph);
        var renderer = new CapturingRenderer(sceneGraph);
        var windowService = new AutoCloseWindowService();
        var app = new ApplicationHost(
            windowService,
            renderer,
            sceneRuntime,
            new NullAssetService(new EngineRuntimeInfo("AnsEngine", "0.1.0"), windowService),
            new StubMeshAssetProvider(),
            new ScriptSceneDescriptionLoader(
                MoveOnInputScript.kScriptId,
                new Dictionary<string, SceneScriptPropertyValue>
                {
                    ["speedUnitsPerSecond"] = SceneScriptPropertyValue.FromNumber(2.0d)
                }),
            "script.scene.json",
            new StubInputService(InputSnapshot.Empty),
            new FixedTimeService(new TimeSnapshot(1.0, 1.0, 1.0)),
            CreateMoveOnInputRuntime());

        var exitCode = app.Run();

        Assert.Equal(0, exitCode);
        AssertVectorNearlyEqual(Vector3.Zero, renderer.FirstRenderedPosition!.Value);
    }

    [Fact]
    public void ApplicationHost_Run_MoveOnInputSingleKeyMovesInWorldDirectionBeforeRender()
    {
        var sceneGraph = new SceneGraphService(new EngineRuntimeInfo("AnsEngine", "0.1.0"));
        var sceneRuntime = CreateSceneRuntime(sceneGraph);
        var renderer = new CapturingRenderer(sceneGraph);
        var windowService = new AutoCloseWindowService();
        var app = new ApplicationHost(
            windowService,
            renderer,
            sceneRuntime,
            new NullAssetService(new EngineRuntimeInfo("AnsEngine", "0.1.0"), windowService),
            new StubMeshAssetProvider(),
            new ScriptSceneDescriptionLoader(
                MoveOnInputScript.kScriptId,
                new Dictionary<string, SceneScriptPropertyValue>
                {
                    ["speedUnitsPerSecond"] = SceneScriptPropertyValue.FromNumber(2.0d)
                }),
            "script.scene.json",
            new StubInputService(InputSnapshot.FromKeys(EngineKey.W)),
            new FixedTimeService(new TimeSnapshot(0.5, 0.5, 2.0)),
            CreateMoveOnInputRuntime());

        var exitCode = app.Run();

        Assert.Equal(0, exitCode);
        AssertVectorNearlyEqual(new Vector3(0.0f, 0.0f, -1.0f), renderer.FirstRenderedPosition!.Value);
    }

    [Fact]
    public void ApplicationHost_Run_MoveOnInputDiagonalMovementIsNormalizedAndPreservesRotationScale()
    {
        var sceneGraph = new SceneGraphService(new EngineRuntimeInfo("AnsEngine", "0.1.0"));
        var sceneRuntime = CreateSceneRuntime(sceneGraph);
        var renderer = new CapturingRenderer(sceneGraph);
        var windowService = new AutoCloseWindowService();
        var app = new ApplicationHost(
            windowService,
            renderer,
            sceneRuntime,
            new NullAssetService(new EngineRuntimeInfo("AnsEngine", "0.1.0"), windowService),
            new StubMeshAssetProvider(),
            new ScriptSceneDescriptionLoader(
                MoveOnInputScript.kScriptId,
                new Dictionary<string, SceneScriptPropertyValue>
                {
                    ["speedUnitsPerSecond"] = SceneScriptPropertyValue.FromNumber(1.0d)
                }),
            "script.scene.json",
            new StubInputService(InputSnapshot.FromKeys(EngineKey.W, EngineKey.D)),
            new FixedTimeService(new TimeSnapshot(1.0, 1.0, 1.0)),
            CreateMoveOnInputRuntime());

        var exitCode = app.Run();

        var diagonal = MathF.Sqrt(0.5f);
        Assert.Equal(0, exitCode);
        AssertVectorNearlyEqual(new Vector3(diagonal, 0.0f, -diagonal), renderer.FirstRenderedPosition!.Value);
        AssertQuaternionNearlyEqual(Quaternion.Identity, renderer.FirstRenderedRotation!.Value);
        AssertVectorNearlyEqual(Vector3.One, renderer.FirstRenderedScale!.Value);
    }

    [Fact]
    public void ApplicationHost_Run_ConvertsPlatformInputToScriptingInputBeforeRender()
    {
        var callLog = new List<string>();
        var script = new LoggingInputScript(callLog);
        var registry = new ScriptRegistry();
        Assert.Null(registry.Register("LogInput", () => script));
        var app = new ApplicationHost(
            new AutoCloseWindowService(callLog),
            new CountingRenderer(callLog),
            CreateSceneRuntime(new SceneGraphService(new EngineRuntimeInfo("AnsEngine", "0.1.0"))),
            new NullAssetService(new EngineRuntimeInfo("AnsEngine", "0.1.0"), new AutoCloseWindowService()),
            new StubMeshAssetProvider(),
            new ScriptSceneDescriptionLoader("LogInput", new Dictionary<string, SceneScriptPropertyValue>()),
            "script.scene.json",
            new StubInputService(InputSnapshot.FromKeys(EngineKey.D), callLog),
            new StubTimeService(new TimeSnapshot(0.25, 1.25, 4.0), callLog),
            new ScriptRuntime(registry));

        var exitCode = app.Run();

        Assert.Equal(0, exitCode);
        Assert.NotNull(script.LastInput);
        Assert.True(script.LastInput.Value.IsKeyDown(ScriptKey.D));
        Assert.False(script.LastInput.Value.IsKeyDown(ScriptKey.W));
        Assert.True(callLog.IndexOf("SceneUpdate") < callLog.IndexOf("ScriptUpdate"));
        Assert.True(callLog.IndexOf("ScriptUpdate") < callLog.IndexOf("RenderFrame"));
    }

    [Fact]
    public void ApplicationHost_Run_UnknownScriptIdFailsCleanlyBeforeRender()
    {
        var windowService = new TrackingWindowService();
        var renderer = new CountingRenderer();
        var app = new ApplicationHost(
            windowService,
            renderer,
            CreateSceneRuntime(new SceneGraphService(new EngineRuntimeInfo("AnsEngine", "0.1.0"))),
            new NullAssetService(new EngineRuntimeInfo("AnsEngine", "0.1.0"), windowService),
            new StubMeshAssetProvider(),
            new ScriptSceneDescriptionLoader("MissingScript"),
            "script.scene.json",
            new NullInputService(),
            new FixedTimeService(new TimeSnapshot(0.016, 0.016, 60)),
            CreateRotateSelfRuntime());

        var exitCode = app.Run();

        Assert.Equal(1, exitCode);
        Assert.Equal(1, renderer.InitializeCalls);
        Assert.Equal(0, renderer.RenderCalls);
        Assert.Equal(1, renderer.ShutdownCalls);
        Assert.Equal(1, windowService.DisposeCalls);
    }

    [Fact]
    public void ApplicationHost_Run_ScriptUpdateExceptionFailsCleanlyBeforeRender()
    {
        var windowService = new TrackingWindowService();
        var renderer = new CountingRenderer();
        var registry = new ScriptRegistry();
        Assert.Null(registry.Register("ThrowOnUpdate", static () => new ThrowingUpdateScript()));
        var app = new ApplicationHost(
            windowService,
            renderer,
            CreateSceneRuntime(new SceneGraphService(new EngineRuntimeInfo("AnsEngine", "0.1.0"))),
            new NullAssetService(new EngineRuntimeInfo("AnsEngine", "0.1.0"), windowService),
            new StubMeshAssetProvider(),
            new ScriptSceneDescriptionLoader("ThrowOnUpdate"),
            "script.scene.json",
            new NullInputService(),
            new FixedTimeService(new TimeSnapshot(0.016, 0.016, 60)),
            new ScriptRuntime(registry));

        var exitCode = app.Run();

        Assert.Equal(1, exitCode);
        Assert.Equal(1, renderer.InitializeCalls);
        Assert.Equal(0, renderer.RenderCalls);
        Assert.Equal(1, renderer.ShutdownCalls);
        Assert.Equal(1, windowService.DisposeCalls);
    }

    [Fact]
    public void ApplicationHost_UsesSceneRuntimeAbstractionWithoutRuntimeInternals()
    {
        var sceneRuntimeField = typeof(ApplicationHost).GetField("mSceneRuntime", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(sceneRuntimeField);
        Assert.Equal(typeof(ISceneRuntime), sceneRuntimeField!.FieldType);

        var fields = typeof(ApplicationHost).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.DoesNotContain(fields, field => field.FieldType == typeof(SceneGraphService));
        Assert.DoesNotContain(fields, field => field.FieldType.Name.Contains("RuntimeScene", StringComparison.Ordinal));
        Assert.DoesNotContain(fields, field => field.FieldType.Name.Contains("SceneRuntimeObject", StringComparison.Ordinal));
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
            new StubMeshAssetProvider(),
            new SuccessfulSceneDescriptionLoader(),
            "sample.scene.json",
            new NullInputService(),
            new FixedTimeService(new TimeSnapshot(0.016, 0, 60)));

        var exitCode = app.Run();

        Assert.Equal(1, exitCode);
        Assert.True(sceneRuntime.InitializeCalled);
        Assert.Equal(1, renderer.InitializeCalls);
        Assert.Equal(1, renderer.RenderCalls);
        Assert.Equal(1, sceneRuntime.UpdateCalls);
        Assert.Equal(1, renderer.ShutdownCalls);
        Assert.Equal(1, windowService.RequestCloseCalls);
        Assert.Equal(1, windowService.DisposeCalls);
        Assert.True(windowService.IsCloseRequested);
    }

    [Fact]
    public void ApplicationHost_Run_LoaderFailure_ReturnsErrorAndSkipsSceneInitialization()
    {
        var windowService = new TrackingWindowService();
        var renderer = new CountingRenderer();
        var sceneRuntime = new SpySceneRuntime();
        var app = new ApplicationHost(
            windowService,
            renderer,
            sceneRuntime,
            new NullAssetService(new EngineRuntimeInfo("AnsEngine", "0.1.0"), windowService),
            new StubMeshAssetProvider(),
            new FailingSceneDescriptionLoader(),
            "missing.scene.json",
            new NullInputService(),
            new FixedTimeService(new TimeSnapshot(0.016, 0, 60)));

        var exitCode = app.Run();

        Assert.Equal(1, exitCode);
        Assert.False(sceneRuntime.InitializeCalled);
        Assert.Equal(0, sceneRuntime.UpdateCalls);
        Assert.Equal(1, renderer.InitializeCalls);
        Assert.Equal(0, renderer.RenderCalls);
        Assert.Equal(1, renderer.ShutdownCalls);
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

    private static void AssertQuaternionNearlyEqual(Quaternion expected, Quaternion actual)
    {
        Assert.Equal(expected.X, actual.X, 5);
        Assert.Equal(expected.Y, actual.Y, 5);
        Assert.Equal(expected.Z, actual.Z, 5);
        Assert.Equal(expected.W, actual.W, 5);
    }

    private static void AssertVectorNearlyEqual(Vector3 expected, Vector3 actual)
    {
        Assert.Equal(expected.X, actual.X, 5);
        Assert.Equal(expected.Y, actual.Y, 5);
        Assert.Equal(expected.Z, actual.Z, 5);
    }

    private static ISceneRuntime CreateSceneRuntime(SceneGraphService sceneGraph)
    {
        var adapterType = typeof(RuntimeBootstrap).Assembly.GetType("Engine.App.SceneRuntimeAdapter");
        Assert.NotNull(adapterType);
        return Assert.IsAssignableFrom<ISceneRuntime>(
            Activator.CreateInstance(adapterType!, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object[] { sceneGraph }, null));
    }

    private static ScriptRuntime CreateRotateSelfRuntime()
    {
        var registry = new ScriptRegistry();
        Assert.Null(registry.Register(RotateSelfScript.kScriptId, static () => new RotateSelfScript()));
        return new ScriptRuntime(registry);
    }

    private static ScriptRuntime CreateMoveOnInputRuntime()
    {
        var registry = new ScriptRegistry();
        Assert.Null(registry.Register(MoveOnInputScript.kScriptId, static () => new MoveOnInputScript()));
        return new ScriptRuntime(registry);
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

        public void Dispose() { }
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

        public Quaternion? FirstRenderedRotation { get; private set; }

        public Vector3? FirstRenderedPosition { get; private set; }

        public Vector3? FirstRenderedScale { get; private set; }

        public void Initialize()
        {
        }

        public void RenderFrame()
        {
            RenderCalls += 1;
            var transform = Assert.Single(mProvider.BuildRenderFrame().Items).Transform;
            FirstRenderedPosition ??= transform.Position;
            FirstRenderedRotation ??= transform.Rotation;
            FirstRenderedScale ??= transform.Scale;
        }

        public void Shutdown()
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

    private sealed class SpySceneRuntime : ISceneRuntime
    {
        private readonly List<string>? mCallLog;

        public SpySceneRuntime(List<string>? callLog = null)
        {
            mCallLog = callLog;
        }

        public bool InitializeCalled { get; private set; }
        public SceneDescription? SceneDescription { get; private set; }
        public int UpdateCalls { get; private set; }
        public TimeSnapshot LastTime { get; private set; }
        public InputSnapshot LastInput { get; private set; }

        public void InitializeScene(SceneDescription sceneDescription)
        {
            InitializeCalled = true;
            SceneDescription = sceneDescription;
        }

        public SceneScriptObjectBindResult BindScriptObject(string objectId)
        {
            throw new NotSupportedException("Spy scene runtime does not support script binding.");
        }

        public void Update(TimeSnapshot time, InputSnapshot input)
        {
            UpdateCalls += 1;
            LastTime = time;
            LastInput = input;
            mCallLog?.Add("SceneUpdate");
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

    private sealed class ScriptSceneDescriptionLoader : ISceneDescriptionLoader
    {
        private readonly string mScriptId;
        private readonly IReadOnlyDictionary<string, SceneScriptPropertyValue>? mProperties;

        public ScriptSceneDescriptionLoader(
            string scriptId,
            IReadOnlyDictionary<string, SceneScriptPropertyValue>? properties = null)
        {
            mScriptId = scriptId;
            mProperties = properties;
        }

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
                                    mScriptId,
                                    mProperties ?? new Dictionary<string, SceneScriptPropertyValue>
                                    {
                                        ["speedRadiansPerSecond"] = SceneScriptPropertyValue.FromNumber(1.0d)
                                    })
                            })
                    }));
        }
    }

    private sealed class ThrowingUpdateScript : IScriptBehavior
    {
        public void Initialize(ScriptContext context)
        {
        }

        public void Update(ScriptContext context)
        {
            throw new InvalidOperationException("update failed");
        }
    }

    private sealed class LoggingInputScript : IScriptBehavior
    {
        private readonly List<string> mCallLog;

        public LoggingInputScript(List<string> callLog)
        {
            mCallLog = callLog;
        }

        public ScriptInputSnapshot? LastInput { get; private set; }

        public void Initialize(ScriptContext context)
        {
        }

        public void Update(ScriptContext context)
        {
            LastInput = context.Input;
            mCallLog.Add("ScriptUpdate");
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
