using Engine.Contracts;
using Engine.SceneData;

namespace Engine.Scene;

internal sealed class SceneMeshRendererComponent
{
    public SceneMeshRendererComponent(SceneMeshRef mesh, SceneMaterialRef material)
    {
        Mesh = mesh;
        Material = material;
    }

    public SceneMeshRef Mesh { get; }

    public SceneMaterialRef Material { get; }

    public static SceneMeshRendererComponent FromDescription(SceneObjectDescription objectDescription)
    {
        ArgumentNullException.ThrowIfNull(objectDescription);
        return new SceneMeshRendererComponent(objectDescription.Mesh, objectDescription.Material);
    }
}
