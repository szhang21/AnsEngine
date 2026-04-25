using Engine.Render;
using Xunit;

namespace Engine.Render.Tests;

public sealed class SceneRenderGpuMeshResourceCacheTests
{
    [Fact]
    public void GetOrCreate_SameMeshKey_ReusesExistingResource()
    {
        var cache = new SceneRenderGpuMeshResourceCache<object>();
        var createCount = 0;

        var first = cache.GetOrCreate("mesh://shared", () =>
        {
            createCount += 1;
            return new object();
        });
        var second = cache.GetOrCreate("mesh://shared", () =>
        {
            createCount += 1;
            return new object();
        });

        Assert.Same(first, second);
        Assert.Equal(1, createCount);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void Clear_ReleasesAllCachedResources()
    {
        var cache = new SceneRenderGpuMeshResourceCache<object>();
        var releaseCount = 0;

        _ = cache.GetOrCreate("mesh://one", () => new object());
        _ = cache.GetOrCreate("mesh://two", () => new object());

        cache.Clear(_ => releaseCount += 1);

        Assert.Equal(2, releaseCount);
        Assert.Equal(0, cache.Count);
    }
}
