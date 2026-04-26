using Engine.Contracts;
using Engine.SceneData.Abstractions;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Engine.SceneData;

public sealed class JsonSceneDescriptionLoader : ISceneDescriptionLoader
{
    private const string kDefaultMaterialId = "material://default";
    private const float kDefaultFieldOfViewRadians = 1.0471976f;
    private static readonly SceneCameraDescription sDefaultCamera = new(
        new Vector3(0.0f, 0.0f, 2.2f),
        Vector3.Zero,
        kDefaultFieldOfViewRadians);
    private static readonly JsonSerializerOptions sJsonOptions = CreateJsonOptions();

    public SceneDescriptionLoadResult Load(string sceneFilePath)
    {
        if (string.IsNullOrWhiteSpace(sceneFilePath))
        {
            return Failure(
                SceneDescriptionLoadFailureKind.MissingRequiredField,
                "Scene file path must not be null or whitespace.",
                sceneFilePath);
        }

        if (!File.Exists(sceneFilePath))
        {
            return Failure(
                SceneDescriptionLoadFailureKind.NotFound,
                $"Scene file '{sceneFilePath}' was not found.",
                sceneFilePath);
        }

        string json;
        try
        {
            json = File.ReadAllText(sceneFilePath);
        }
        catch (Exception ex)
        {
            return Failure(
                SceneDescriptionLoadFailureKind.InvalidValue,
                $"Scene file '{sceneFilePath}' could not be read: {ex.Message}",
                sceneFilePath);
        }

        SceneFileDocument? document;
        try
        {
            document = JsonSerializer.Deserialize<SceneFileDocument>(json, sJsonOptions);
        }
        catch (JsonException ex)
        {
            return Failure(
                SceneDescriptionLoadFailureKind.InvalidJson,
                $"Scene file '{sceneFilePath}' contains invalid JSON: {ex.Message}",
                sceneFilePath,
                ex.LineNumber is null ? null : (int?)ex.LineNumber.Value);
        }

        if (document is null)
        {
            return Failure(
                SceneDescriptionLoadFailureKind.InvalidJson,
                $"Scene file '{sceneFilePath}' did not deserialize into a scene document.",
                sceneFilePath);
        }

        return Normalize(document, sceneFilePath);
    }

    private static SceneDescriptionLoadResult Normalize(SceneFileDocument document, string sceneFilePath)
    {
        if (string.IsNullOrWhiteSpace(document.Version))
        {
            return Failure(
                SceneDescriptionLoadFailureKind.MissingRequiredField,
                "Scene document version is required.",
                sceneFilePath);
        }

        if (document.Scene is null)
        {
            return Failure(
                SceneDescriptionLoadFailureKind.MissingRequiredField,
                "Scene definition is required.",
                sceneFilePath);
        }

        if (string.IsNullOrWhiteSpace(document.Scene.Id))
        {
            return Failure(
                SceneDescriptionLoadFailureKind.MissingRequiredField,
                "Scene id is required.",
                sceneFilePath);
        }

        if (document.Scene.Objects is null)
        {
            return Failure(
                SceneDescriptionLoadFailureKind.MissingRequiredField,
                "Scene objects collection is required.",
                sceneFilePath);
        }

        var objectIds = new HashSet<string>(StringComparer.Ordinal);
        var objects = new List<SceneObjectDescription>(document.Scene.Objects.Count);

        for (var index = 0; index < document.Scene.Objects.Count; index += 1)
        {
            var objectDefinition = document.Scene.Objects[index];
            var objectResult = NormalizeObject(objectDefinition, objectIds, sceneFilePath, index);
            if (!objectResult.IsSuccess)
            {
                return objectResult;
            }

            objects.Add(objectResult.Scene!.Objects[0]);
        }

        var cameraResult = NormalizeCamera(document.Scene.Camera, sceneFilePath);
        if (!cameraResult.IsSuccess)
        {
            return cameraResult;
        }

        return SceneDescriptionLoadResult.Success(
            new SceneDescription(
                document.Scene.Id,
                string.IsNullOrWhiteSpace(document.Scene.Name) ? document.Scene.Id : document.Scene.Name,
                cameraResult.Scene!.Camera,
                objects));
    }

    private static SceneDescriptionLoadResult NormalizeObject(
        SceneFileObjectDefinition objectDefinition,
        HashSet<string> objectIds,
        string sceneFilePath,
        int objectIndex)
    {
        if (string.IsNullOrWhiteSpace(objectDefinition.Id))
        {
            return Failure(
                SceneDescriptionLoadFailureKind.MissingRequiredField,
                $"Scene object at index {objectIndex} is missing required field 'id'.",
                sceneFilePath);
        }

        if (!objectIds.Add(objectDefinition.Id))
        {
            return Failure(
                SceneDescriptionLoadFailureKind.DuplicateObjectId,
                $"Scene object id '{objectDefinition.Id}' is duplicated.",
                sceneFilePath);
        }

        if (string.IsNullOrWhiteSpace(objectDefinition.Mesh))
        {
            return Failure(
                SceneDescriptionLoadFailureKind.MissingRequiredField,
                $"Scene object '{objectDefinition.Id}' is missing required field 'mesh'.",
                sceneFilePath);
        }

        SceneMeshRef mesh;
        try
        {
            mesh = new SceneMeshRef(objectDefinition.Mesh);
        }
        catch (ArgumentException ex)
        {
            return Failure(
                SceneDescriptionLoadFailureKind.InvalidReference,
                $"Scene object '{objectDefinition.Id}' has invalid mesh reference: {ex.Message}",
                sceneFilePath);
        }

        var materialId = string.IsNullOrWhiteSpace(objectDefinition.Material)
            ? kDefaultMaterialId
            : objectDefinition.Material;

        SceneMaterialRef material;
        try
        {
            material = new SceneMaterialRef(materialId);
        }
        catch (ArgumentException ex)
        {
            return Failure(
                SceneDescriptionLoadFailureKind.InvalidReference,
                $"Scene object '{objectDefinition.Id}' has invalid material reference: {ex.Message}",
                sceneFilePath);
        }

        var transformResult = NormalizeTransform(objectDefinition.Transform, sceneFilePath, objectDefinition.Id);
        if (!transformResult.IsSuccess)
        {
            return transformResult;
        }

        return SceneDescriptionLoadResult.Success(
            new SceneDescription(
                "object-normalization",
                "object-normalization",
                sDefaultCamera,
                new[]
                {
                    new SceneObjectDescription(
                        objectDefinition.Id,
                        string.IsNullOrWhiteSpace(objectDefinition.Name) ? objectDefinition.Id : objectDefinition.Name,
                        mesh,
                        material,
                        transformResult.Scene!.Objects[0].LocalTransform)
                }));
    }

    private static SceneDescriptionLoadResult NormalizeTransform(
        SceneFileTransformDefinition? transformDefinition,
        string sceneFilePath,
        string objectId)
    {
        var position = transformDefinition?.Position ?? Vector3.Zero;
        var rotation = transformDefinition?.Rotation ?? Quaternion.Identity;
        var scale = transformDefinition?.Scale ?? Vector3.One;

        if (!IsFinite(position) || !IsFinite(scale) || !IsFinite(rotation))
        {
            return Failure(
                SceneDescriptionLoadFailureKind.InvalidValue,
                $"Scene object '{objectId}' contains non-finite transform values.",
                sceneFilePath);
        }

        return SceneDescriptionLoadResult.Success(
            new SceneDescription(
                "transform-normalization",
                "transform-normalization",
                sDefaultCamera,
                new[]
                {
                    new SceneObjectDescription(
                        objectId,
                        objectId,
                        new SceneMeshRef("mesh://placeholder"),
                        new SceneMaterialRef(kDefaultMaterialId),
                        new SceneTransformDescription(position, rotation, scale))
                }));
    }

    private static SceneDescriptionLoadResult NormalizeCamera(
        SceneFileCameraDefinition? cameraDefinition,
        string sceneFilePath)
    {
        if (cameraDefinition is null)
        {
            return SceneDescriptionLoadResult.Success(
                new SceneDescription(
                    "camera-normalization",
                    "camera-normalization",
                    sDefaultCamera,
                    Array.Empty<SceneObjectDescription>()));
        }

        var position = cameraDefinition.Position;
        var target = cameraDefinition.Target;
        var fieldOfViewRadians = cameraDefinition.FieldOfViewRadians ?? kDefaultFieldOfViewRadians;

        if (!IsFinite(position) || !IsFinite(target) || !float.IsFinite(fieldOfViewRadians) || fieldOfViewRadians <= 0.0f)
        {
            return Failure(
                SceneDescriptionLoadFailureKind.InvalidValue,
                "Scene camera contains invalid values.",
                sceneFilePath);
        }

        return SceneDescriptionLoadResult.Success(
            new SceneDescription(
                "camera-normalization",
                "camera-normalization",
                new SceneCameraDescription(position, target, fieldOfViewRadians),
                Array.Empty<SceneObjectDescription>()));
    }

    private static SceneDescriptionLoadResult Failure(
        SceneDescriptionLoadFailureKind kind,
        string message,
        string path,
        int? lineNumber = null)
    {
        var normalizedPath = string.IsNullOrWhiteSpace(path) ? "<unknown>" : path;
        return SceneDescriptionLoadResult.FailureResult(
            new SceneDescriptionLoadFailure(kind, message, normalizedPath, lineNumber));
    }

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.X) &&
               float.IsFinite(value.Y) &&
               float.IsFinite(value.Z);
    }

    private static bool IsFinite(Quaternion value)
    {
        return float.IsFinite(value.X) &&
               float.IsFinite(value.Y) &&
               float.IsFinite(value.Z) &&
               float.IsFinite(value.W);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
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
