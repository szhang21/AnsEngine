using System.Text.Json;
using System.Text.Json.Serialization;

namespace Engine.SceneData;

[JsonConverter(typeof(SceneFileScriptPropertyValueJsonConverter))]
public readonly record struct SceneFileScriptPropertyValue
{
    private SceneFileScriptPropertyValue(double? number, bool? boolean, string? text)
    {
        Number = number;
        Boolean = boolean;
        Text = text;
    }

    public double? Number { get; }

    public bool? Boolean { get; }

    public string? Text { get; }

    public bool IsNumber => Number is not null;

    public bool IsBoolean => Boolean is not null;

    public bool IsString => Text is not null;

    public static SceneFileScriptPropertyValue FromNumber(double value)
    {
        return new SceneFileScriptPropertyValue(value, null, null);
    }

    public static SceneFileScriptPropertyValue FromBoolean(bool value)
    {
        return new SceneFileScriptPropertyValue(null, value, null);
    }

    public static SceneFileScriptPropertyValue FromString(string value)
    {
        return new SceneFileScriptPropertyValue(null, null, value ?? string.Empty);
    }

    public static SceneFileScriptPropertyValue FromJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => FromNumber(element.GetDouble()),
            JsonValueKind.True => FromBoolean(true),
            JsonValueKind.False => FromBoolean(false),
            JsonValueKind.String => FromString(element.GetString() ?? string.Empty),
            _ => throw new JsonException("Script property values must be number, bool, or string.")
        };
    }
}

internal sealed class SceneFileScriptPropertyValueJsonConverter : JsonConverter<SceneFileScriptPropertyValue>
{
    public override SceneFileScriptPropertyValue Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        return SceneFileScriptPropertyValue.FromJsonElement(document.RootElement);
    }

    public override void Write(
        Utf8JsonWriter writer,
        SceneFileScriptPropertyValue value,
        JsonSerializerOptions options)
    {
        if (value.IsNumber)
        {
            writer.WriteNumberValue(value.Number!.Value);
            return;
        }

        if (value.IsBoolean)
        {
            writer.WriteBooleanValue(value.Boolean!.Value);
            return;
        }

        if (value.IsString)
        {
            writer.WriteStringValue(value.Text);
            return;
        }

        throw new JsonException("Unsupported script property value.");
    }
}
