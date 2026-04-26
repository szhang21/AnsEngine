using Engine.Contracts;
using System.Numerics;

namespace Engine.Render;

public readonly record struct SceneRenderMeshVertex(
    float X,
    float Y,
    float Z,
    float NormalX,
    float NormalY,
    float NormalZ);

public readonly record struct SceneRenderVertex(
    float X,
    float Y,
    float Z,
    float R,
    float G,
    float B);

public readonly record struct SceneRenderMaterialParameters(
    float Red,
    float Green,
    float Blue);

public sealed record SceneRenderBatch
{
    public SceneRenderBatch(
        string meshCacheKey,
        IReadOnlyList<SceneRenderMeshVertex> meshVertices,
        SceneRenderMaterialParameters material,
        Matrix4x4 modelViewProjection)
    {
        if (string.IsNullOrWhiteSpace(meshCacheKey))
        {
            throw new ArgumentException("Mesh cache key must not be null or whitespace.", nameof(meshCacheKey));
        }

        MeshVertices = meshVertices ?? throw new ArgumentNullException(nameof(meshVertices));
        MeshCacheKey = meshCacheKey;
        Material = material;
        ModelViewProjection = modelViewProjection;
        Vertices = BuildVertices(meshVertices, material);
    }

    public string MeshCacheKey { get; }

    public IReadOnlyList<SceneRenderMeshVertex> MeshVertices { get; }

    public SceneRenderMaterialParameters Material { get; }

    public IReadOnlyList<SceneRenderVertex> Vertices { get; }

    public Matrix4x4 ModelViewProjection { get; }

    private static IReadOnlyList<SceneRenderVertex> BuildVertices(
        IReadOnlyList<SceneRenderMeshVertex> meshVertices,
        SceneRenderMaterialParameters material)
    {
        if (meshVertices.Count == 0)
        {
            return Array.Empty<SceneRenderVertex>();
        }

        var vertices = new SceneRenderVertex[meshVertices.Count];
        for (var index = 0; index < meshVertices.Count; index += 1)
        {
            var meshVertex = meshVertices[index];
            vertices[index] = new SceneRenderVertex(
                meshVertex.X,
                meshVertex.Y,
                meshVertex.Z,
                material.Red,
                material.Green,
                material.Blue);
        }

        return vertices;
    }
}

public sealed record SceneRenderSubmission(IReadOnlyList<SceneRenderBatch> Batches);

public static class SceneRenderSubmissionBuilder
{
    private const float kTriangleHalfWidth = 0.22f;
    private const float kTriangleHalfHeight = 0.20f;
    private const string kTriangleFallbackMeshId = "fallback://triangle";
    private const string kDefaultMaterialId = "material://default";
    private static readonly SceneRenderMaterialParameters sDefaultMaterial =
        new(0.72f, 0.78f, 0.86f);
    private static readonly SceneRenderMeshGeometryCache sMeshGeometryCache = new(CreateFallbackGeometry());
    private static readonly IReadOnlyDictionary<string, SceneRenderMaterialParameters> sMaterials =
        new Dictionary<string, SceneRenderMaterialParameters>(StringComparer.Ordinal)
        {
            [kDefaultMaterialId] = sDefaultMaterial,
            ["material://pulse"] = new SceneRenderMaterialParameters(0.98f, 0.54f, 0.32f),
            ["material://highlight"] = new SceneRenderMaterialParameters(0.42f, 0.85f, 0.58f)
        };

    public static SceneRenderSubmission Build(SceneRenderFrame frame, IMeshAssetProvider? meshAssetProvider = null)
    {
        ArgumentNullException.ThrowIfNull(frame);

        if (frame.Items.Count == 0)
        {
            return new SceneRenderSubmission(Array.Empty<SceneRenderBatch>());
        }

        var batches = new List<SceneRenderBatch>(frame.Items.Count);
        for (var index = 0; index < frame.Items.Count; index += 1)
        {
            var item = frame.Items[index];
            var material = ResolveMaterial(item.MaterialId);
            var meshGeometry = sMeshGeometryCache.Resolve(item.Mesh, meshAssetProvider);
            var modelMatrix = BuildTransformMatrix(item.Transform);
            var modelViewProjection = modelMatrix * frame.Camera.View * frame.Camera.Projection;
            batches.Add(new SceneRenderBatch(meshGeometry.MeshCacheKey, meshGeometry.Vertices, material, modelViewProjection));
        }

        return new SceneRenderSubmission(batches);
    }

    private static SceneRenderMaterialParameters ResolveMaterial(string materialId)
    {
        return sMaterials.TryGetValue(materialId, out var material)
            ? material
            : sDefaultMaterial;
    }

    private static Matrix4x4 BuildTransformMatrix(SceneTransform transform)
    {
        if (transform.Equals(SceneTransform.Identity))
        {
            return Matrix4x4.Identity;
        }

        var scale = Matrix4x4.CreateScale(transform.Scale);
        var rotation = Matrix4x4.CreateFromQuaternion(transform.Rotation);
        var translation = Matrix4x4.CreateTranslation(transform.Position);
        return scale * rotation * translation;
    }

    private static SceneRenderMeshGeometry CreateFallbackGeometry()
    {
        return new SceneRenderMeshGeometry(
            kTriangleFallbackMeshId,
            new[]
            {
                new SceneRenderMeshVertex(0f, kTriangleHalfHeight, 0f, 0f, 0f, 1f),
                new SceneRenderMeshVertex(-kTriangleHalfWidth, -kTriangleHalfHeight, 0f, 0f, 0f, 1f),
                new SceneRenderMeshVertex(kTriangleHalfWidth, -kTriangleHalfHeight, 0f, 0f, 0f, 1f)
            });
    }
}
