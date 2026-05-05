using Engine.Asset;
using Engine.Contracts;
using Engine.Core;
using Engine.Render;
using Engine.Scene;
using Engine.SceneData;

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
        Snapshot = new EditorScenePreviewSnapshot(
            true,
            frame.Items.Count > 0 && submission.Batches.Count > 0 && vertexCount > 0,
            frame.Items.Count,
            submission.Batches.Count,
            vertexCount,
            mRefreshVersion,
            frame.Items.Count > 0 ? "Preview ready." : "Scene has no renderable objects.");
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
