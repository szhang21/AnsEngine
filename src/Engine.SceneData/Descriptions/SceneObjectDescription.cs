using Engine.Contracts;

namespace Engine.SceneData;

public sealed record SceneObjectDescription
{
    public SceneObjectDescription(
        string objectId,
        string objectName,
        IReadOnlyList<SceneComponentDescription> components)
    {
        ObjectId = objectId;
        ObjectName = objectName;
        Components = components;
    }

    public SceneObjectDescription(
        string objectId,
        string objectName,
        SceneMeshRef mesh,
        SceneMaterialRef material,
        SceneTransformDescription localTransform)
        : this(
            objectId,
            objectName,
            new SceneComponentDescription[]
            {
                new SceneTransformComponentDescription(localTransform),
                new SceneMeshRendererComponentDescription(mesh, material)
            })
    {
    }

    public string ObjectId { get; init; }

    public string ObjectName { get; init; }

    public IReadOnlyList<SceneComponentDescription> Components { get; init; }

    public SceneTransformComponentDescription? TransformComponent =>
        Components.OfType<SceneTransformComponentDescription>().FirstOrDefault();

    public SceneMeshRendererComponentDescription? MeshRendererComponent =>
        Components.OfType<SceneMeshRendererComponentDescription>().FirstOrDefault();

    public IReadOnlyList<SceneScriptComponentDescription> ScriptComponents =>
        Components.OfType<SceneScriptComponentDescription>().ToArray();

    public SceneMeshRef Mesh => MeshRendererComponent?.Mesh ?? new SceneMeshRef("mesh://missing");

    public SceneMaterialRef Material => MeshRendererComponent?.Material ?? new SceneMaterialRef("material://default");

    public SceneTransformDescription LocalTransform => TransformComponent?.Transform ?? SceneTransformDescription.Identity;
}
