using Engine.Contracts;
using Engine.Scripting;
using System.Numerics;
using Xunit;

namespace Engine.Scripting.Tests;

public sealed class ScriptRuntimeTests
{
    [Fact]
    public void Bind_ValidScript_InitializesExactlyOnce()
    {
        var script = new CountingScript();
        var runtime = CreateRuntime("test.script", script);

        var result = runtime.Bind(CreateBindings("test.script"));

        Assert.True(result.IsSuccess, result.Failure?.Message);
        Assert.Equal(1, runtime.BoundScriptCount);
        Assert.Equal(1, script.InitializeCount);
        Assert.Equal(0, script.UpdateCount);
        Assert.Equal("cube-main", script.LastContext!.ObjectId);
    }

    [Fact]
    public void Update_BoundScript_UpdatesEveryFrameWithTiming()
    {
        var script = new CountingScript();
        var runtime = CreateRuntime("test.script", script);
        Assert.True(runtime.Bind(CreateBindings("test.script")).IsSuccess);

        Assert.True(runtime.Update(0.25d, 0.25d).IsSuccess);
        Assert.True(runtime.Update(0.5d, 0.75d).IsSuccess);

        Assert.Equal(1, script.InitializeCount);
        Assert.Equal(2, script.UpdateCount);
        Assert.Equal(0.5d, script.LastContext!.DeltaSeconds);
        Assert.Equal(0.75d, script.LastContext.TotalSeconds);
        Assert.Equal(ScriptInputSnapshot.Empty, script.LastContext.Input);
    }

    [Fact]
    public void Update_WithFrameInput_PassesInputToBoundScript()
    {
        var script = new CountingScript();
        var runtime = CreateRuntime("test.script", script);
        Assert.True(runtime.Bind(CreateBindings("test.script")).IsSuccess);

        var input = ScriptInputSnapshot.FromKeys(ScriptKey.W, ScriptKey.D);
        var result = runtime.Update(0.25d, 0.25d, input);

        Assert.True(result.IsSuccess, result.Failure?.Message);
        Assert.Equal(input, script.LastContext!.Input);
        Assert.True(script.LastContext.Input.AnyInputDetected);
        Assert.True(script.LastContext.Input.IsKeyDown(ScriptKey.W));
        Assert.True(script.LastContext.Input.IsKeyDown(ScriptKey.D));
        Assert.False(script.LastContext.Input.IsKeyDown(ScriptKey.A));
        Assert.False(script.LastContext.Input.IsKeyDown(ScriptKey.S));
    }

    [Fact]
    public void Update_WithFrameInput_PassesSameInputToEveryScriptInBindingOrder()
    {
        var firstScript = new CountingScript();
        var secondScript = new CountingScript();
        var registry = new ScriptRegistry();
        Assert.Null(registry.Register("first.script", () => firstScript));
        Assert.Null(registry.Register("second.script", () => secondScript));
        var runtime = new ScriptRuntime(registry);
        var bindings = new[]
        {
            CreateBinding("first.script", "cube-a"),
            CreateBinding("second.script", "cube-b")
        };
        Assert.True(runtime.Bind(bindings).IsSuccess);

        var input = ScriptInputSnapshot.FromKeys(ScriptKey.A, ScriptKey.S);
        var result = runtime.Update(0.5d, 1.0d, input);

        Assert.True(result.IsSuccess, result.Failure?.Message);
        Assert.Equal(input, firstScript.LastContext!.Input);
        Assert.Equal(input, secondScript.LastContext!.Input);
        Assert.Equal(1, firstScript.UpdateCount);
        Assert.Equal(1, secondScript.UpdateCount);
    }

    [Fact]
    public void Bind_MissingScriptId_ReturnsFailureAndDoesNotBind()
    {
        var runtime = new ScriptRuntime(new ScriptRegistry());

        var result = runtime.Bind(CreateBindings("missing.script"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ScriptFailureKind.MissingScriptId, result.Failure!.Kind);
        Assert.Equal("cube-main", result.Failure.ObjectId);
        Assert.Equal(0, runtime.BoundScriptCount);
    }

    [Fact]
    public void Bind_InvalidProperty_ReturnsFailureAndDoesNotBind()
    {
        var runtime = CreateRuntime("test.script", new CountingScript());
        var bindings = CreateBindings(
            "test.script",
            new Dictionary<string, ScriptPropertyValue>
            {
                ["speed"] = ScriptPropertyValue.FromNumber(double.NaN)
            });

        var result = runtime.Bind(bindings);

        Assert.False(result.IsSuccess);
        Assert.Equal(ScriptFailureKind.InvalidProperty, result.Failure!.Kind);
        Assert.Equal("speed", result.Failure.PropertyName);
        Assert.Equal(0, runtime.BoundScriptCount);
    }

    [Fact]
    public void Bind_ScriptInitializeException_ReturnsDiagnosticFailure()
    {
        var runtime = CreateRuntime("test.script", new ThrowingScript(throwOnInitialize: true));

        var result = runtime.Bind(CreateBindings("test.script"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ScriptFailureKind.ScriptException, result.Failure!.Kind);
        Assert.Equal("test.script", result.Failure.ScriptId);
        Assert.Equal("cube-main", result.Failure.ObjectId);
        Assert.Contains("initialize failed", result.Failure.Message);
        Assert.Equal(0, runtime.BoundScriptCount);
    }

    [Fact]
    public void Update_ScriptUpdateException_ReturnsDiagnosticFailure()
    {
        var runtime = CreateRuntime("test.script", new ThrowingScript(throwOnUpdate: true));
        Assert.True(runtime.Bind(CreateBindings("test.script")).IsSuccess);

        var result = runtime.Update(0.25d, 0.25d);

        Assert.False(result.IsSuccess);
        Assert.Equal(ScriptFailureKind.ScriptException, result.Failure!.Kind);
        Assert.Equal("test.script", result.Failure.ScriptId);
        Assert.Equal("cube-main", result.Failure.ObjectId);
        Assert.Contains("update failed", result.Failure.Message);
    }

    [Fact]
    public void Update_ScriptCanChangeOnlyProvidedSelfObjectTransform()
    {
        var script = new RotateSelfScript();
        var self = new TestSelfObject();
        var runtime = CreateRuntime("rotate-self", script);
        Assert.True(runtime.Bind(CreateBindings("rotate-self", self: self)).IsSuccess);

        var result = runtime.Update(1.0d, 1.0d);

        Assert.True(result.IsSuccess, result.Failure?.Message);
        Assert.Equal(
            Quaternion.Normalize(Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI * 0.5f)),
            self.Transform.LocalTransform.Rotation);
    }

    [Fact]
    public void ScriptInputSnapshot_EmptyAndNonEmptyStates_AreStable()
    {
        var empty = ScriptInputSnapshot.Empty;
        var input = ScriptInputSnapshot.FromKeys(ScriptKey.W, ScriptKey.A);

        Assert.False(empty.AnyInputDetected);
        Assert.False(empty.IsKeyDown(ScriptKey.W));
        Assert.True(input.AnyInputDetected);
        Assert.True(input.IsKeyDown(ScriptKey.W));
        Assert.True(input.IsKeyDown(ScriptKey.A));
        Assert.False(input.IsKeyDown(ScriptKey.S));
        Assert.False(input.IsKeyDown(ScriptKey.D));
    }

    [Fact]
    public void ScriptPropertyReader_RequiredValues_ReturnsTypedProperties()
    {
        var context = CreateContext(
            new Dictionary<string, ScriptPropertyValue>
            {
                ["speed"] = ScriptPropertyValue.FromNumber(2.5d),
                ["enabled"] = ScriptPropertyValue.FromBoolean(true),
                ["label"] = ScriptPropertyValue.FromString("main")
            });

        Assert.Equal(2.5d, ScriptPropertyReader.RequireNumber(context, "speed"));
        Assert.True(ScriptPropertyReader.RequireBoolean(context, "enabled"));
        Assert.Equal("main", ScriptPropertyReader.RequireString(context, "label"));
    }

    [Fact]
    public void ScriptPropertyReader_MissingOrWrongType_ThrowsStableFailure()
    {
        var context = CreateContext(
            new Dictionary<string, ScriptPropertyValue>
            {
                ["speed"] = ScriptPropertyValue.FromString("fast"),
                ["enabled"] = ScriptPropertyValue.FromNumber(1.0d),
                ["label"] = ScriptPropertyValue.FromBoolean(true)
            });

        var missing = Assert.Throws<InvalidOperationException>(() => ScriptPropertyReader.RequireNumber(context, "missing"));
        var badNumber = Assert.Throws<InvalidOperationException>(() => ScriptPropertyReader.RequireNumber(context, "speed"));
        var badBoolean = Assert.Throws<InvalidOperationException>(() => ScriptPropertyReader.RequireBoolean(context, "enabled"));
        var badString = Assert.Throws<InvalidOperationException>(() => ScriptPropertyReader.RequireString(context, "label"));

        Assert.Contains("missing required property 'missing'", missing.Message);
        Assert.Contains("requires finite numeric property 'speed'", badNumber.Message);
        Assert.Contains("requires boolean property 'enabled'", badBoolean.Message);
        Assert.Contains("requires string property 'label'", badString.Message);
    }

    [Fact]
    public void Bind_NonFiniteProperty_ReturnsDeterministicInvalidPropertyBeforeInitialize()
    {
        var script = new CountingScript();
        var runtime = CreateRuntime("test.script", script);
        var bindings = CreateBindings(
            "test.script",
            new Dictionary<string, ScriptPropertyValue>
            {
                ["speed"] = ScriptPropertyValue.FromNumber(double.PositiveInfinity)
            });

        var result = runtime.Bind(bindings);

        Assert.False(result.IsSuccess);
        Assert.Equal(ScriptFailureKind.InvalidProperty, result.Failure!.Kind);
        Assert.Equal("speed", result.Failure.PropertyName);
        Assert.Equal(0, script.InitializeCount);
        Assert.Equal(0, runtime.BoundScriptCount);
    }

    private static ScriptRuntime CreateRuntime(string scriptId, IScriptBehavior script)
    {
        var registry = new ScriptRegistry();
        Assert.Null(registry.Register(scriptId, () => script));
        return new ScriptRuntime(registry);
    }

    [Fact]
    public void EngineScripting_ProjectDoesNotReferenceScene()
    {
        var projectFile = File.ReadAllText(FindRepositoryFile("src", "Engine.Scripting", "Engine.Scripting.csproj"));
        var sourceText = string.Join(
            '\n',
            Directory.GetFiles(FindRepositoryDirectory("src", "Engine.Scripting"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                .Select(File.ReadAllText));

        Assert.DoesNotContain("Engine.Scene", projectFile);
        Assert.DoesNotContain("Engine.Scene", sourceText);
    }

    private static IReadOnlyList<ScriptBindingDescription> CreateBindings(
        string scriptId,
        IReadOnlyDictionary<string, ScriptPropertyValue>? properties = null,
        IScriptSelfObject? self = null)
    {
        return new[]
        {
            CreateBinding(scriptId, "cube-main", properties, self)
        };
    }

    private static ScriptBindingDescription CreateBinding(
        string scriptId,
        string objectId,
        IReadOnlyDictionary<string, ScriptPropertyValue>? properties = null,
        IScriptSelfObject? self = null)
    {
        return new ScriptBindingDescription(
            objectId,
            objectId,
            self ?? new TestSelfObject(),
            scriptId,
            properties ?? new Dictionary<string, ScriptPropertyValue>
            {
                ["speed"] = ScriptPropertyValue.FromNumber(1.0d),
                ["enabled"] = ScriptPropertyValue.FromBoolean(true),
                ["label"] = ScriptPropertyValue.FromString("main")
            });
    }

    private static ScriptContext CreateContext(IReadOnlyDictionary<string, ScriptPropertyValue> properties)
    {
        return new ScriptContext(
            "cube-main",
            "Cube Main",
            new TestSelfObject(),
            properties,
            0.0d,
            0.0d,
            ScriptInputSnapshot.Empty);
    }

    private sealed class CountingScript : IScriptBehavior
    {
        public int InitializeCount { get; private set; }

        public int UpdateCount { get; private set; }

        public ScriptContext? LastContext { get; private set; }

        public void Initialize(ScriptContext context)
        {
            InitializeCount += 1;
            LastContext = context;
        }

        public void Update(ScriptContext context)
        {
            UpdateCount += 1;
            LastContext = context;
        }
    }

    private sealed class ThrowingScript : IScriptBehavior
    {
        private readonly bool mThrowOnInitialize;
        private readonly bool mThrowOnUpdate;

        public ThrowingScript(bool throwOnInitialize = false, bool throwOnUpdate = false)
        {
            mThrowOnInitialize = throwOnInitialize;
            mThrowOnUpdate = throwOnUpdate;
        }

        public void Initialize(ScriptContext context)
        {
            if (mThrowOnInitialize)
            {
                throw new InvalidOperationException("initialize failed");
            }
        }

        public void Update(ScriptContext context)
        {
            if (mThrowOnUpdate)
            {
                throw new InvalidOperationException("update failed");
            }
        }
    }

    private sealed class RotateSelfScript : IScriptBehavior
    {
        public void Initialize(ScriptContext context)
        {
        }

        public void Update(ScriptContext context)
        {
            var rotationDelta = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)context.DeltaSeconds * MathF.PI * 0.5f);
            var transform = context.Self.Transform.LocalTransform;
            _ = ScriptPropertyReader.RequireNumber(context, "speed");
            context.Self.Transform.SetLocalTransform(transform with
            {
                Rotation = Quaternion.Normalize(rotationDelta * transform.Rotation)
            });
        }
    }

    private sealed class TestSelfObject : IScriptSelfObject
    {
        public TestTransformComponent Transform { get; } = new();

        IScriptTransformComponent IScriptSelfObject.Transform => Transform;
    }

    private sealed class TestTransformComponent : IScriptTransformComponent
    {
        public SceneTransform LocalTransform { get; private set; } = SceneTransform.Identity;

        public void SetLocalTransform(SceneTransform transform)
        {
            LocalTransform = transform;
        }
    }

    private static string FindRepositoryFile(params string[] relativeSegments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(relativeSegments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(relativeSegments));
    }

    private static string FindRepositoryDirectory(params string[] relativeSegments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(relativeSegments).ToArray());
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException($"Could not locate repository directory '{Path.Combine(relativeSegments)}'.");
    }
}
