using Engine.Contracts;
using System.Numerics;

namespace Engine.Render;

public readonly record struct SceneRenderMeshVertex(
    float X,
    float Y,
    float Z);

public readonly record struct SceneRenderVertex(
    float X,
    float Y,
    float Z,
    float R,
    float G,
    float B);

public sealed record SceneRenderBatch(
    IReadOnlyList<SceneRenderVertex> Vertices,
    Matrix4x4 ModelViewProjection);

public sealed record SceneRenderSubmission(IReadOnlyList<SceneRenderBatch> Batches);

public static class SceneRenderSubmissionBuilder
{
    private const float TriangleHalfWidth = 0.22f;
    private const float TriangleHalfHeight = 0.20f;
    private const float BaseY = -0.15f;
    private const string TriangleMeshId = "mesh://triangle";
    private static readonly IReadOnlyDictionary<string, SceneRenderMeshVertex[]> Meshes =
        new Dictionary<string, SceneRenderMeshVertex[]>
        {
            [TriangleMeshId] =
            [
                new SceneRenderMeshVertex(0f, TriangleHalfHeight, 0f),
                new SceneRenderMeshVertex(-TriangleHalfWidth, -TriangleHalfHeight, 0f),
                new SceneRenderMeshVertex(TriangleHalfWidth, -TriangleHalfHeight, 0f)
            ]
        };

    public static SceneRenderSubmission Build(SceneRenderFrame frame)
    {
        if (frame.Items.Count == 0)
        {
            return new SceneRenderSubmission(Array.Empty<SceneRenderBatch>());
        }

        var batches = new List<SceneRenderBatch>(frame.Items.Count);
        for (var index = 0; index < frame.Items.Count; index += 1)
        {
            var item = frame.Items[index];
            var materialHash = Math.Abs(item.MaterialId.GetHashCode(StringComparison.Ordinal));
            var red = 0.45f + (materialHash % 40) / 100f;
            var green = 0.55f + (materialHash % 30) / 120f;
            var blue = 0.65f + (materialHash % 20) / 140f;
            var meshVertices = ResolveMesh(item.MeshId);
            var vertices = new List<SceneRenderVertex>(meshVertices.Length);
            for (var vertexIndex = 0; vertexIndex < meshVertices.Length; vertexIndex += 1)
            {
                var meshVertex = meshVertices[vertexIndex];
                vertices.Add(new SceneRenderVertex(meshVertex.X, meshVertex.Y, meshVertex.Z, red, green, blue));
            }

            var centerX = -0.65f + (index * 0.6f);
            var layoutMatrix = Matrix4x4.CreateTranslation(centerX, BaseY, 0f);
            var modelMatrix = layoutMatrix * BuildTransformMatrix(item.Transform);
            var modelViewProjection = modelMatrix * frame.Camera.View * frame.Camera.Projection;
            batches.Add(new SceneRenderBatch(vertices, modelViewProjection));
        }

        return new SceneRenderSubmission(batches);
    }

    private static SceneRenderMeshVertex[] ResolveMesh(string meshId)
    {
        return Meshes.TryGetValue(meshId, out var meshVertices)
            ? meshVertices
            : Meshes[TriangleMeshId];
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
}
