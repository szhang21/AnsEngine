namespace Engine.Contracts;

public interface IMeshAssetProvider
{
    MeshAssetLoadResult GetMesh(SceneMeshRef mesh);
}
