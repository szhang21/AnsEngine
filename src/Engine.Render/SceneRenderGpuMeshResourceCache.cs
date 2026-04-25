namespace Engine.Render;

internal sealed class SceneRenderGpuMeshResourceCache<TResource>
    where TResource : class
{
    private readonly Dictionary<string, TResource> resources = new(StringComparer.Ordinal);

    public int Count => resources.Count;

    public TResource GetOrCreate(string meshCacheKey, Func<TResource> factory)
    {
        if (string.IsNullOrWhiteSpace(meshCacheKey))
        {
            throw new ArgumentException("Mesh cache key must not be null or whitespace.", nameof(meshCacheKey));
        }

        ArgumentNullException.ThrowIfNull(factory);

        if (resources.TryGetValue(meshCacheKey, out var resource))
        {
            return resource;
        }

        resource = factory();
        resources.Add(meshCacheKey, resource);
        return resource;
    }

    public void Clear(Action<TResource>? release)
    {
        if (release is not null)
        {
            foreach (var resource in resources.Values)
            {
                release(resource);
            }
        }

        resources.Clear();
    }
}
