using Engine.Contracts;
using System.Numerics;

namespace Engine.SceneData;

public static class SceneFileDocumentNormalizer
{
    private const string kDefaultMaterialId = "material://default";
    private const string kRequiredVersion = "2.0";
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

        if (!string.Equals(document.Version, kRequiredVersion, StringComparison.Ordinal))
        {
            return Failure(
                SceneDescriptionLoadFailureKind.InvalidValue,
                $"Scene document version must be '{kRequiredVersion}'.",
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

        if (objectDefinition.Components is null || objectDefinition.Components.Count == 0)
        {
            return Failure(
                SceneDescriptionLoadFailureKind.MissingRequiredField,
                $"Scene object '{objectDefinition.Id}' is missing required field 'components'.",
                sceneFilePath);
        }

        var transformComponent = GetSingleComponent<SceneFileTransformComponentDefinition>(
            objectDefinition,
            SceneFileComponentTypes.Transform,
            sceneFilePath);
        if (!transformComponent.IsSuccess)
        {
            return SceneDescriptionLoadResult.FailureResult(transformComponent.Failure!);
        }

        if (transformComponent.Component is null)
        {
            return Failure(
                SceneDescriptionLoadFailureKind.MissingRequiredField,
                $"Scene object '{objectDefinition.Id}' is missing required component 'Transform'.",
                sceneFilePath);
        }

        var meshRendererComponent = GetSingleComponent<SceneFileMeshRendererComponentDefinition>(
            objectDefinition,
            SceneFileComponentTypes.MeshRenderer,
            sceneFilePath);
        if (!meshRendererComponent.IsSuccess)
        {
            return SceneDescriptionLoadResult.FailureResult(meshRendererComponent.Failure!);
        }

        var transformResult = NormalizeTransform(transformComponent.Component.ToTransformDefinition(), sceneFilePath, objectDefinition.Id);
        if (!transformResult.IsSuccess)
        {
            return transformResult;
        }

        SceneMeshRendererComponentDescription? normalizedMeshRenderer = null;
        if (meshRendererComponent.Component is not null)
        {
            var meshRendererResult = NormalizeMeshRenderer(meshRendererComponent.Component, sceneFilePath, objectDefinition.Id);
            if (!meshRendererResult.IsSuccess)
            {
                return meshRendererResult.Result;
            }

            normalizedMeshRenderer = meshRendererResult.Component!;
        }

        var components = new List<SceneComponentDescription>();
        foreach (var component in objectDefinition.Components)
        {
            switch (component)
            {
                case SceneFileTransformComponentDefinition:
                    components.Add(new SceneTransformComponentDescription(transformResult.Scene!.Objects[0].LocalTransform));
                    break;

                case SceneFileMeshRendererComponentDefinition when meshRendererComponent.Component is not null:
                    components.Add(normalizedMeshRenderer!);
                    break;

                case SceneFileScriptComponentDefinition scriptComponent:
                    var scriptResult = NormalizeScript(scriptComponent, sceneFilePath, objectDefinition.Id);
                    if (!scriptResult.IsSuccess)
                    {
                        return scriptResult.Result;
                    }

                    components.Add(scriptResult.Component!);
                    break;
            }
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
                        components)
                }));
    }

    private sealed record MeshRendererResult(
        SceneMeshRendererComponentDescription? Component,
        SceneDescriptionLoadResult Result)
    {
        public bool IsSuccess => Result.IsSuccess;
    }

    private sealed record ScriptResult(
        SceneScriptComponentDescription? Component,
        SceneDescriptionLoadResult Result)
    {
        public bool IsSuccess => Result.IsSuccess;
    }

    private static MeshRendererResult NormalizeMeshRenderer(
        SceneFileMeshRendererComponentDefinition meshRenderer,
        string sceneFilePath,
        string objectId)
    {
        if (string.IsNullOrWhiteSpace(meshRenderer.Mesh))
        {
            return MeshRendererFailure(
                SceneDescriptionLoadFailureKind.MissingRequiredField,
                $"Scene object '{objectId}' is missing required field 'mesh'.",
                sceneFilePath);
        }

        if (!IsSceneReferenceId(meshRenderer.Mesh))
        {
            return MeshRendererFailure(
                SceneDescriptionLoadFailureKind.InvalidReference,
                $"Scene object '{objectId}' has invalid mesh reference format.",
                sceneFilePath);
        }

        SceneMeshRef mesh;
        try
        {
            mesh = new SceneMeshRef(meshRenderer.Mesh);
        }
        catch (ArgumentException ex)
        {
            return MeshRendererFailure(
                SceneDescriptionLoadFailureKind.InvalidReference,
                $"Scene object '{objectId}' has invalid mesh reference: {ex.Message}",
                sceneFilePath);
        }

        var materialId = string.IsNullOrWhiteSpace(meshRenderer.Material)
            ? kDefaultMaterialId
            : meshRenderer.Material;

        if (!IsSceneReferenceId(materialId))
        {
            return MeshRendererFailure(
                SceneDescriptionLoadFailureKind.InvalidReference,
                $"Scene object '{objectId}' has invalid material reference format.",
                sceneFilePath);
        }

        SceneMaterialRef material;
        try
        {
            material = new SceneMaterialRef(materialId);
        }
        catch (ArgumentException ex)
        {
            return MeshRendererFailure(
                SceneDescriptionLoadFailureKind.InvalidReference,
                $"Scene object '{objectId}' has invalid material reference: {ex.Message}",
                sceneFilePath);
        }

        return new MeshRendererResult(
            new SceneMeshRendererComponentDescription(mesh, material),
            SceneDescriptionLoadResult.Success(
                new SceneDescription("mesh-renderer-normalization", "mesh-renderer-normalization", sDefaultCamera, Array.Empty<SceneObjectDescription>())));
    }

    private static MeshRendererResult MeshRendererFailure(
        SceneDescriptionLoadFailureKind kind,
        string message,
        string path)
    {
        return new MeshRendererResult(null, Failure(kind, message, path));
    }

    private static ScriptResult NormalizeScript(
        SceneFileScriptComponentDefinition script,
        string sceneFilePath,
        string objectId)
    {
        if (string.IsNullOrWhiteSpace(script.ScriptId))
        {
            return ScriptFailure(
                SceneDescriptionLoadFailureKind.MissingRequiredField,
                $"Scene object '{objectId}' has Script component with missing required field 'scriptId'.",
                sceneFilePath);
        }

        var properties = new Dictionary<string, SceneScriptPropertyValue>(StringComparer.Ordinal);
        foreach (var property in script.Properties)
        {
            if (string.IsNullOrWhiteSpace(property.Key))
            {
                return ScriptFailure(
                    SceneDescriptionLoadFailureKind.InvalidValue,
                    $"Scene object '{objectId}' Script '{script.ScriptId}' has an empty property name.",
                    sceneFilePath);
            }

            var value = property.Value;
            if (value.IsNumber)
            {
                if (!value.Number.HasValue || !double.IsFinite(value.Number.Value))
                {
                    return ScriptFailure(
                        SceneDescriptionLoadFailureKind.InvalidValue,
                        $"Scene object '{objectId}' Script '{script.ScriptId}' property '{property.Key}' must be a finite number.",
                        sceneFilePath);
                }

                properties[property.Key] = SceneScriptPropertyValue.FromNumber(value.Number.Value);
                continue;
            }

            if (value.IsBoolean)
            {
                properties[property.Key] = SceneScriptPropertyValue.FromBoolean(value.Boolean!.Value);
                continue;
            }

            if (value.IsString)
            {
                properties[property.Key] = SceneScriptPropertyValue.FromString(value.Text ?? string.Empty);
                continue;
            }

            return ScriptFailure(
                SceneDescriptionLoadFailureKind.InvalidValue,
                $"Scene object '{objectId}' Script '{script.ScriptId}' property '{property.Key}' has unsupported value type.",
                sceneFilePath);
        }

        return new ScriptResult(
            new SceneScriptComponentDescription(script.ScriptId, properties),
            SceneDescriptionLoadResult.Success(
                new SceneDescription("script-normalization", "script-normalization", sDefaultCamera, Array.Empty<SceneObjectDescription>())));
    }

    private static ScriptResult ScriptFailure(
        SceneDescriptionLoadFailureKind kind,
        string message,
        string path)
    {
        return new ScriptResult(null, Failure(kind, message, path));
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

    private sealed record ComponentResult<TComponent>(TComponent? Component, SceneDescriptionLoadFailure? Failure)
        where TComponent : SceneFileComponentDefinition
    {
        public bool IsSuccess => Failure is null;
    }

    private static ComponentResult<TComponent> GetSingleComponent<TComponent>(
        SceneFileObjectDefinition objectDefinition,
        string componentType,
        string sceneFilePath)
        where TComponent : SceneFileComponentDefinition
    {
        SceneFileComponentDefinition? found = null;
        foreach (var component in objectDefinition.Components)
        {
            if (!IsSupportedComponentType(component.Type))
            {
                return new ComponentResult<TComponent>(
                    null,
                    new SceneDescriptionLoadFailure(
                        SceneDescriptionLoadFailureKind.InvalidValue,
                        $"Scene object '{objectDefinition.Id}' has unknown component type '{component.Type}'.",
                        sceneFilePath));
            }

            if (!string.Equals(component.Type, componentType, StringComparison.Ordinal))
            {
                continue;
            }

            if (found is not null)
            {
                return new ComponentResult<TComponent>(
                    null,
                    new SceneDescriptionLoadFailure(
                        SceneDescriptionLoadFailureKind.InvalidValue,
                        $"Scene object '{objectDefinition.Id}' has duplicate component type '{componentType}'.",
                        sceneFilePath));
            }

            found = component;
        }

        return new ComponentResult<TComponent>((TComponent?)found, null);
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

    private static bool IsSupportedComponentType(string type)
    {
        return string.Equals(type, SceneFileComponentTypes.Transform, StringComparison.Ordinal) ||
               string.Equals(type, SceneFileComponentTypes.MeshRenderer, StringComparison.Ordinal) ||
               string.Equals(type, SceneFileComponentTypes.Script, StringComparison.Ordinal);
    }
}
