using Engine.Scripting;
using Xunit;

namespace Engine.Scripting.Tests;

public sealed class ScriptRegistryTests
{
    [Fact]
    public void RegisterAndCreate_RegisteredScript_ReturnsNewInstance()
    {
        var registry = new ScriptRegistry();

        var failure = registry.Register("test.script", () => new CountingScript());
        var result = registry.Create("test.script");

        Assert.Null(failure);
        Assert.True(result.IsSuccess);
        Assert.IsType<CountingScript>(result.Behavior);
    }

    [Fact]
    public void Register_DuplicateScriptId_ReturnsFailure()
    {
        var registry = new ScriptRegistry();
        Assert.Null(registry.Register("test.script", () => new CountingScript()));

        var failure = registry.Register("test.script", () => new CountingScript());

        Assert.NotNull(failure);
        Assert.Equal(ScriptFailureKind.DuplicateScriptId, failure!.Kind);
        Assert.Equal("test.script", failure.ScriptId);
    }

    [Fact]
    public void Create_MissingScriptId_ReturnsFailure()
    {
        var registry = new ScriptRegistry();

        var result = registry.Create("missing.script");

        Assert.False(result.IsSuccess);
        Assert.Equal(ScriptFailureKind.MissingScriptId, result.Failure!.Kind);
        Assert.Equal("missing.script", result.Failure.ScriptId);
    }

    private sealed class CountingScript : IScriptBehavior
    {
        public void Initialize(ScriptContext context)
        {
        }

        public void Update(ScriptContext context)
        {
        }
    }
}
