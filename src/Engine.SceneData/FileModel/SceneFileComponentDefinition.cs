using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Engine.SceneData;

[JsonConverter(typeof(SceneFileComponentDefinitionJsonConverter))]
public abstract record SceneFileComponentDefinition(string Type);

public sealed record SceneFileTransformComponentDefinition(
    Vector3? Position,
    Quaternion? Rotation,
    Vector3? Scale)
    : SceneFileComponentDefinition(SceneFileComponentTypes.Transform)
{
    public SceneFileTransformComponentDefinition(SceneFileTransformDefinition? transform)
        : this(transform?.Position, transform?.Rotation, transform?.Scale)
    {
    }

    public SceneFileTransformDefinition ToTransformDefinition()
    {
        return new SceneFileTransformDefinition(Position, Rotation, Scale);
    }
}

public sealed record SceneFileMeshRendererComponentDefinition(
    string Mesh,
    string? Material)
    : SceneFileComponentDefinition(SceneFileComponentTypes.MeshRenderer);

public sealed record SceneFileScriptComponentDefinition : SceneFileComponentDefinition
{
    public SceneFileScriptComponentDefinition(
        string scriptId,
        IReadOnlyDictionary<string, SceneFileScriptPropertyValue>? properties)
        : base(SceneFileComponentTypes.Script)
    {
        ScriptId = scriptId;
        Properties = properties ?? new Dictionary<string, SceneFileScriptPropertyValue>(StringComparer.Ordinal);
    }

    public string ScriptId { get; init; }

    public IReadOnlyDictionary<string, SceneFileScriptPropertyValue> Properties { get; init; }
}

public static class SceneFileComponentTypes
{
    public const string Transform = "Transform";
    public const string MeshRenderer = "MeshRenderer";
    public const string Script = "Script";
}

internal sealed class SceneFileComponentDefinitionJsonConverter : JsonConverter<SceneFileComponentDefinition>
{
    public override SceneFileComponentDefinition Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        if (!document.RootElement.TryGetProperty("type", out var typeElement))
        {
            throw new JsonException("Scene component requires a type.");
        }

        var type = typeElement.GetString();
        return type switch
        {
            SceneFileComponentTypes.Transform => new SceneFileTransformComponentDefinition(
                ReadOptional<Vector3>(document.RootElement, "position", options),
                ReadOptional<Quaternion>(document.RootElement, "rotation", options),
                ReadOptional<Vector3>(document.RootElement, "scale", options)),
            SceneFileComponentTypes.MeshRenderer => new SceneFileMeshRendererComponentDefinition(
                ReadString(document.RootElement, "mesh") ?? string.Empty,
                ReadString(document.RootElement, "material")),
            SceneFileComponentTypes.Script => new SceneFileScriptComponentDefinition(
                ReadString(document.RootElement, "scriptId") ?? string.Empty,
                ReadScriptProperties(document.RootElement, "properties")),
            _ => throw new JsonException($"Unknown scene component type '{type}'.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        SceneFileComponentDefinition value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);

        switch (value)
        {
            case SceneFileTransformComponentDefinition transform:
                if (transform.Position is not null)
                {
                    writer.WritePropertyName("position");
                    JsonSerializer.Serialize(writer, transform.Position.Value, options);
                }

                if (transform.Rotation is not null)
                {
                    writer.WritePropertyName("rotation");
                    JsonSerializer.Serialize(writer, transform.Rotation.Value, options);
                }

                if (transform.Scale is not null)
                {
                    writer.WritePropertyName("scale");
                    JsonSerializer.Serialize(writer, transform.Scale.Value, options);
                }

                break;

            case SceneFileMeshRendererComponentDefinition meshRenderer:
                writer.WriteString("mesh", meshRenderer.Mesh);
                if (meshRenderer.Material is not null)
                {
                    writer.WriteString("material", meshRenderer.Material);
                }

                break;

            case SceneFileScriptComponentDefinition script:
                writer.WriteString("scriptId", script.ScriptId);
                if (script.Properties.Count > 0)
                {
                    writer.WritePropertyName("properties");
                    JsonSerializer.Serialize(writer, script.Properties, options);
                }

                break;

            default:
                throw new JsonException($"Unsupported scene component type '{value.GetType().Name}'.");
        }

        writer.WriteEndObject();
    }

    private static T? ReadOptional<T>(JsonElement element, string propertyName, JsonSerializerOptions options)
        where T : struct
    {
        return element.TryGetProperty(propertyName, out var property)
            ? property.Deserialize<T>(options)
            : null;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property)
            ? property.GetString()
            : null;
    }

    private static IReadOnlyDictionary<string, SceneFileScriptPropertyValue> ReadScriptProperties(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var propertiesElement))
        {
            return new Dictionary<string, SceneFileScriptPropertyValue>(StringComparer.Ordinal);
        }

        if (propertiesElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Script properties must be an object.");
        }

        var properties = new Dictionary<string, SceneFileScriptPropertyValue>(StringComparer.Ordinal);
        foreach (var property in propertiesElement.EnumerateObject())
        {
            properties[property.Name] = SceneFileScriptPropertyValue.FromJsonElement(property.Value);
        }

        return properties;
    }
}
