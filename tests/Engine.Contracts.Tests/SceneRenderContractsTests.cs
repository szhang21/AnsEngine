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
        Assert.Equal("mesh://triangle", frame.Items[0].Mesh.MeshId);
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

    [Fact]
    public void SceneRenderItem_WithResourceRefs_PreservesStructuredResources()
    {
        var mesh = new SceneMeshRef("mesh://quad");
        var material = new SceneMaterialRef("material://lit");
        var item = new SceneRenderItem(9, mesh, material);

        Assert.Equal(mesh, item.Mesh);
        Assert.Equal(material, item.Material);
        Assert.Equal("mesh://quad", item.MeshId);
        Assert.Equal("material://lit", item.MaterialId);
        Assert.Equal(SceneTransform.Identity, item.Transform);
    }

    [Fact]
    public void SceneMeshRef_EmptyId_ThrowsArgumentException()
    {
        _ = Assert.Throws<ArgumentException>(() => new SceneMeshRef(" "));
    }

    [Fact]
    public void SceneMaterialRef_EmptyId_ThrowsArgumentException()
    {
        _ = Assert.Throws<ArgumentException>(() => new SceneMaterialRef(string.Empty));
    }

    [Fact]
    public void SceneRenderFrame_WithoutCamera_UsesIdentityCamera()
    {
        var frame = new SceneRenderFrame(
            3,
            new[]
            {
                new SceneRenderItem(1, "mesh://triangle", "material://default")
            });

        Assert.Equal(Matrix4x4.Identity, frame.Camera.View);
        Assert.Equal(Matrix4x4.Identity, frame.Camera.Projection);
    }

    [Fact]
    public void SceneRenderFrame_WithCamera_PreservesViewProjection()
    {
        var view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 2), Vector3.Zero, Vector3.UnitY);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 3f, 16f / 9f, 0.1f, 100f);
        var expectedCamera = new SceneCamera(view, projection);
        var frame = new SceneRenderFrame(
            4,
            new[]
            {
                new SceneRenderItem(2, "mesh://triangle", "material://default")
            },
            expectedCamera);

        Assert.Equal(expectedCamera, frame.Camera);
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
