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
    public void Update_ScriptCanChangeOnlyProvidedSelfTransform()
    {
        var script = new RotateSelfScript();
        var transform = new TestTransform();
        var runtime = CreateRuntime("rotate-self", script);
        Assert.True(runtime.Bind(CreateBindings("rotate-self", selfTransform: transform)).IsSuccess);

        var result = runtime.Update(1.0d, 1.0d);

        Assert.True(result.IsSuccess, result.Failure?.Message);
        Assert.Equal(
            Quaternion.Normalize(Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI * 0.5f)),
            transform.LocalTransform.Rotation);
    }

    private static ScriptRuntime CreateRuntime(string scriptId, IScriptBehavior script)
    {
        var registry = new ScriptRegistry();
        Assert.Null(registry.Register(scriptId, () => script));
        return new ScriptRuntime(registry);
    }

    private static IReadOnlyList<ScriptBindingDescription> CreateBindings(
        string scriptId,
        IReadOnlyDictionary<string, ScriptPropertyValue>? properties = null,
        IScriptSelfTransform? selfTransform = null)
    {
        return new[]
        {
            new ScriptBindingDescription(
                "cube-main",
                "Cube Main",
                selfTransform ?? new TestTransform(),
                scriptId,
                properties ?? new Dictionary<string, ScriptPropertyValue>
                {
                    ["speed"] = ScriptPropertyValue.FromNumber(1.0d),
                    ["enabled"] = ScriptPropertyValue.FromBoolean(true),
                    ["label"] = ScriptPropertyValue.FromString("main")
                })
        };
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
            var transform = context.SelfTransform.LocalTransform;
            context.SelfTransform.SetLocalTransform(transform with
            {
                Rotation = Quaternion.Normalize(rotationDelta * transform.Rotation)
            });
        }
    }

    private sealed class TestTransform : IScriptSelfTransform
    {
        public SceneTransform LocalTransform { get; private set; } = SceneTransform.Identity;

        public void SetLocalTransform(SceneTransform transform)
        {
            LocalTransform = transform;
        }
    }
}
