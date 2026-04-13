using Engine.Contracts;
using Xunit;

namespace Engine.Contracts.Tests;

public sealed class SceneRenderContractsTests
{
    [Fact]
    public void SceneRenderFrame_StoresItems()
    {
        var items = new[]
        {
            new SceneRenderItem(1, "mesh://triangle", "material://default")
        };

        var frame = new SceneRenderFrame(7, items);

        Assert.Equal(7, frame.FrameNumber);
        Assert.Single(frame.Items);
        Assert.Equal("mesh://triangle", frame.Items[0].MeshId);
    }

    [Fact]
    public void ContractProvider_ReturnsFrame()
    {
        ISceneRenderContractProvider provider = new TestProvider();

        var frame = provider.BuildRenderFrame();

        Assert.Single(frame.Items);
        Assert.Equal("material://default", frame.Items[0].MaterialId);
    }

    private sealed class TestProvider : ISceneRenderContractProvider
    {
        public SceneRenderFrame BuildRenderFrame()
        {
            return new SceneRenderFrame(
                0,
                new[]
                {
                    new SceneRenderItem(1, "mesh://triangle", "material://default")
                });
        }
    }
}
