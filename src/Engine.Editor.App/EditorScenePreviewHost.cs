using Engine.Asset;
using Engine.Contracts;
using Engine.Core;
using Engine.Render;
using Engine.Scene;
using Engine.SceneData;
using System.Numerics;

namespace Engine.Editor.App;

internal sealed class EditorScenePreviewHost
{
    private const string kSampleMeshCatalogFileName = "mesh-catalog.txt";
    private readonly SceneGraphService mSceneGraph;
    private readonly IMeshAssetProvider mMeshAssetProvider;
    private int mRefreshVersion;

    public EditorScenePreviewHost(SceneGraphService sceneGraph, IMeshAssetProvider meshAssetProvider)
    {
        mSceneGraph = sceneGraph ?? throw new ArgumentNullException(nameof(sceneGraph));
        mMeshAssetProvider = meshAssetProvider ?? throw new ArgumentNullException(nameof(meshAssetProvider));
    }

    public EditorScenePreviewSnapshot Snapshot { get; private set; } = EditorScenePreviewSnapshot.Empty;

    public static EditorScenePreviewHost CreateDefault()
    {
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine.Editor.Preview", "0.1.0");
        return new EditorScenePreviewHost(
            new SceneGraphService(runtimeInfo),
            new DiskMeshAssetProvider(ResolveSampleMeshCatalogPath()));
    }

    public void Refresh(SceneDescription? scene)
    {
        mRefreshVersion += 1;
        if (scene is null)
        {
            Snapshot = EditorScenePreviewSnapshot.Empty with
            {
                RefreshVersion = mRefreshVersion
            };
            return;
        }

        mSceneGraph.LoadSceneDescription(scene);
        var frame = mSceneGraph.BuildRenderFrame();
        var submission = SceneRenderSubmissionBuilder.Build(frame, mMeshAssetProvider);
        var vertexCount = submission.Batches.Sum(batch => batch.MeshVertices.Count);
        var projectedTriangles = ProjectTriangles(submission);
        Snapshot = new EditorScenePreviewSnapshot(
            true,
            frame.Items.Count > 0 && submission.Batches.Count > 0 && projectedTriangles.Count > 0,
            frame.Items.Count,
            submission.Batches.Count,
            vertexCount,
            projectedTriangles,
            mRefreshVersion,
            frame.Items.Count > 0 ? "Preview ready." : "Scene has no renderable objects.");
    }

    private static IReadOnlyList<EditorScenePreviewTriangle> ProjectTriangles(SceneRenderSubmission submission)
    {
        var triangles = new List<EditorScenePreviewTriangle>();
        foreach (var batch in submission.Batches)
        {
            for (var index = 0; index + 2 < batch.MeshVertices.Count; index += 3)
            {
                if (!TryProject(batch.MeshVertices[index], batch.ModelViewProjection, out var first) ||
                    !TryProject(batch.MeshVertices[index + 1], batch.ModelViewProjection, out var second) ||
                    !TryProject(batch.MeshVertices[index + 2], batch.ModelViewProjection, out var third))
                {
                    continue;
                }

                triangles.Add(
                    new EditorScenePreviewTriangle(
                        first,
                        second,
                        third,
                        new Vector3(batch.Material.Red, batch.Material.Green, batch.Material.Blue)));
            }
        }

        return triangles;
    }

    private static bool TryProject(SceneRenderMeshVertex vertex, Matrix4x4 modelViewProjection, out Vector2 projected)
    {
        var clip = Vector4.Transform(new Vector4(vertex.X, vertex.Y, vertex.Z, 1.0f), modelViewProjection);
        if (MathF.Abs(clip.W) <= float.Epsilon)
        {
            projected = Vector2.Zero;
            return false;
        }

        projected = new Vector2(clip.X / clip.W, clip.Y / clip.W);
        return IsFinite(projected.X) && IsFinite(projected.Y);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static string ResolveSampleMeshCatalogPath()
    {
        foreach (var startDirectory in EnumerateSearchRoots())
        {
            var directory = new DirectoryInfo(startDirectory);
            while (directory is not null)
            {
                var candidate = Path.Combine(
                    directory.FullName,
                    "src",
                    "Engine.App",
                    "SampleAssets",
                    kSampleMeshCatalogFileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }
        }

        throw new FileNotFoundException("Could not locate sample mesh catalog for editor scene preview.");
    }

    private static IEnumerable<string> EnumerateSearchRoots()
    {
        yield return AppContext.BaseDirectory;
        yield return Directory.GetCurrentDirectory();
    }
}
