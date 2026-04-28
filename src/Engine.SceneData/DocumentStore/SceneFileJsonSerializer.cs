using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Engine.SceneData;

internal static class SceneFileJsonSerializer
{
    private static readonly JsonSerializerOptions sJsonOptions = CreateJsonOptions();

    public static SceneFileDocument? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<SceneFileDocument>(json, sJsonOptions);
    }

    public static string Serialize(SceneFileDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return JsonSerializer.Serialize(document, sJsonOptions);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        options.Converters.Add(new Vector3JsonConverter());
        options.Converters.Add(new QuaternionJsonConverter());
        return options;
    }

    private sealed class Vector3JsonConverter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected an object for Vector3.");
            }

            float? x = null;
            float? y = null;
            float? z = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (x is null || y is null || z is null)
                    {
                        throw new JsonException("Vector3 requires x, y and z.");
                    }

                    return new Vector3(x.Value, y.Value, z.Value);
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Unexpected token in Vector3.");
                }

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "x":
                    case "X":
                        x = reader.GetSingle();
                        break;
                    case "y":
                    case "Y":
                        y = reader.GetSingle();
                        break;
                    case "z":
                    case "Z":
                        z = reader.GetSingle();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonException("Unexpected end of JSON while reading Vector3.");
        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.X);
            writer.WriteNumber("y", value.Y);
            writer.WriteNumber("z", value.Z);
            writer.WriteEndObject();
        }
    }

    private sealed class QuaternionJsonConverter : JsonConverter<Quaternion>
    {
        public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected an object for Quaternion.");
            }

            float? x = null;
            float? y = null;
            float? z = null;
            float? w = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (x is null || y is null || z is null || w is null)
                    {
                        throw new JsonException("Quaternion requires x, y, z and w.");
                    }

                    return new Quaternion(x.Value, y.Value, z.Value, w.Value);
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Unexpected token in Quaternion.");
                }

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "x":
                    case "X":
                        x = reader.GetSingle();
                        break;
                    case "y":
                    case "Y":
                        y = reader.GetSingle();
                        break;
                    case "z":
                    case "Z":
                        z = reader.GetSingle();
                        break;
                    case "w":
                    case "W":
                        w = reader.GetSingle();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonException("Unexpected end of JSON while reading Quaternion.");
        }

        public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.X);
            writer.WriteNumber("y", value.Y);
            writer.WriteNumber("z", value.Z);
            writer.WriteNumber("w", value.W);
            writer.WriteEndObject();
        }
    }
}
