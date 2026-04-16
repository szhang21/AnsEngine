using Engine.Contracts;
using System.Numerics;
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

    [Fact]
    public void SceneRenderItem_WithoutTransform_UsesIdentityTransform()
    {
        var item = new SceneRenderItem(1, "mesh://triangle", "material://default");

        Assert.Equal(Vector3.Zero, item.Transform.Position);
        Assert.Equal(Vector3.One, item.Transform.Scale);
        Assert.Equal(Quaternion.Identity, item.Transform.Rotation);
    }

    [Fact]
    public void SceneRenderItem_WithTransform_PreservesRotation()
    {
        var expectedTransform = new SceneTransform(
            new Vector3(1.5f, -2.0f, 3.25f),
            new Vector3(2.0f, 0.5f, 1.0f),
            Quaternion.CreateFromYawPitchRoll(0.3f, 0.6f, -0.2f));
        var item = new SceneRenderItem(7, "mesh://triangle", "material://default", expectedTransform);

        Assert.Equal(expectedTransform, item.Transform);
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
