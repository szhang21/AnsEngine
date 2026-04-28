using Engine.Contracts;
using System.Numerics;

namespace Engine.SceneData;

public static class SceneFileDocumentNormalizer
{
    private const string kDefaultMaterialId = "material://default";
    private const float kDefaultFieldOfViewRadians = 1.0471976f;
    private static readonly SceneCameraDescription sDefaultCamera = new(
        new Vector3(0.0f, 0.0f, 2.2f),
        Vector3.Zero,
        kDefaultFieldOfViewRadians);

    public static SceneDescriptionLoadResult Normalize(SceneFileDocument document, string sceneFilePath)
    {
        ArgumentNullException.ThrowIfNull(document);

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

        if (!IsSceneReferenceId(objectDefinition.Mesh))
        {
            return Failure(
                SceneDescriptionLoadFailureKind.InvalidReference,
                $"Scene object '{objectDefinition.Id}' has invalid mesh reference format.",
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

        if (!IsSceneReferenceId(materialId))
        {
            return Failure(
                SceneDescriptionLoadFailureKind.InvalidReference,
                $"Scene object '{objectDefinition.Id}' has invalid material reference format.",
                sceneFilePath);
        }

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
        string path)
    {
        var normalizedPath = string.IsNullOrWhiteSpace(path) ? "<unknown>" : path;
        return SceneDescriptionLoadResult.FailureResult(
            new SceneDescriptionLoadFailure(kind, message, normalizedPath));
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

    private static bool IsSceneReferenceId(string value)
    {
        var separatorIndex = value.IndexOf("://", StringComparison.Ordinal);
        return separatorIndex > 0 && separatorIndex < value.Length - 3;
    }
}
