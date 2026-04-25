namespace Engine.Render;

internal sealed class SceneRenderGpuMeshResource
{
    public SceneRenderGpuMeshResource(int vertexArrayHandle, int vertexBufferHandle, int vertexCount)
    {
        VertexArrayHandle = vertexArrayHandle;
        VertexBufferHandle = vertexBufferHandle;
        VertexCount = vertexCount;
    }

    public int VertexArrayHandle { get; }

    public int VertexBufferHandle { get; }

    public int VertexCount { get; }
}
