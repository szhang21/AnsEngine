using Engine.Core;

namespace Engine.Core.Tests;

public sealed class RuntimeInfoTests
{
    [Fact]
    public void Constructor_SetsNameAndVersion()
    {
        var info = new EngineRuntimeInfo("AnsEngine", "0.1.0");

        Assert.Equal("AnsEngine", info.EngineName);
        Assert.Equal("0.1.0", info.Version);
    }
}
