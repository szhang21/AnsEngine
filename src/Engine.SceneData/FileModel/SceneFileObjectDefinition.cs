namespace Engine.SceneData;

using System.Text.Json.Serialization;

public sealed record SceneFileObjectDefinition
{
    [JsonConstructor]
    public SceneFileObjectDefinition(
        string id,
        string name,
        IReadOnlyList<SceneFileComponentDefinition>? components)
    {
        Id = id;
        Name = name;
        Components = components ?? Array.Empty<SceneFileComponentDefinition>();
    }

    public SceneFileObjectDefinition(
        string Id,
        string Name,
        string Mesh,
        string? Material,
        SceneFileTransformDefinition? Transform)
        : this(
            Id,
            Name,
            new SceneFileComponentDefinition[]
            {
                new SceneFileTransformComponentDefinition(Transform),
                new SceneFileMeshRendererComponentDefinition(Mesh, Material)
            })
    {
    }

    public string Id { get; init; }

    public string Name { get; init; }

    public IReadOnlyList<SceneFileComponentDefinition> Components { get; init; }

    [JsonIgnore]
    public string Mesh
    {
        get => FindMeshRenderer()?.Mesh ?? string.Empty;
        init => Components = ReplaceMeshRenderer(value, Material);
    }

    [JsonIgnore]
    public string? Material
    {
        get => FindMeshRenderer()?.Material;
        init => Components = ReplaceMeshRenderer(Mesh, value);
    }

    [JsonIgnore]
    public SceneFileTransformDefinition? Transform
    {
        get => FindTransform()?.ToTransformDefinition();
        init => Components = ReplaceTransform(value);
    }

    public SceneFileTransformComponentDefinition? FindTransform()
    {
        return Components.OfType<SceneFileTransformComponentDefinition>().FirstOrDefault();
    }

    public SceneFileMeshRendererComponentDefinition? FindMeshRenderer()
    {
        return Components.OfType<SceneFileMeshRendererComponentDefinition>().FirstOrDefault();
    }

    private IReadOnlyList<SceneFileComponentDefinition> ReplaceTransform(SceneFileTransformDefinition? transform)
    {
        return ReplaceComponent(new SceneFileTransformComponentDefinition(transform));
    }

    private IReadOnlyList<SceneFileComponentDefinition> ReplaceMeshRenderer(string mesh, string? material)
    {
        return ReplaceComponent(new SceneFileMeshRendererComponentDefinition(mesh, material));
    }

    private IReadOnlyList<SceneFileComponentDefinition> ReplaceComponent(SceneFileComponentDefinition component)
    {
        var components = Components
            .Where(item => !string.Equals(item.Type, component.Type, StringComparison.Ordinal))
            .Concat(new[] { component })
            .ToArray();
        return components;
    }
}
