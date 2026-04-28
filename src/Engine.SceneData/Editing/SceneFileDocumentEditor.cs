namespace Engine.SceneData;

public sealed class SceneFileDocumentEditor
{
    public SceneDocumentEditResult AddObject(
        SceneFileDocument document,
        SceneFileObjectDefinition objectDefinition)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(objectDefinition);

        if (ContainsObject(document, objectDefinition.Id))
        {
            return Failure(
                SceneDocumentEditFailureKind.DuplicateObjectId,
                $"Scene object id '{objectDefinition.Id}' is duplicated.");
        }

        var objects = document.Scene.Objects.Concat(new[] { objectDefinition }).ToArray();
        return ValidateAndReturn(WithObjects(document, objects));
    }

    public SceneDocumentEditResult RemoveObject(SceneFileDocument document, string objectId)
    {
        ArgumentNullException.ThrowIfNull(document);

        var objectIndex = FindObjectIndex(document, objectId);
        if (objectIndex < 0)
        {
            return Failure(
                SceneDocumentEditFailureKind.ObjectNotFound,
                $"Scene object id '{objectId}' was not found.");
        }

        var objects = document.Scene.Objects
            .Where((_, index) => index != objectIndex)
            .ToArray();
        return ValidateAndReturn(WithObjects(document, objects));
    }

    public SceneDocumentEditResult UpdateObjectId(
        SceneFileDocument document,
        string currentObjectId,
        string newObjectId)
    {
        return UpdateObject(
            document,
            currentObjectId,
            item => item with { Id = newObjectId },
            newObjectId);
    }

    public SceneDocumentEditResult UpdateObjectName(
        SceneFileDocument document,
        string objectId,
        string name)
    {
        return UpdateObject(document, objectId, item => item with { Name = name });
    }

    public SceneDocumentEditResult UpdateObjectResources(
        SceneFileDocument document,
        string objectId,
        string mesh,
        string? material)
    {
        return UpdateObject(document, objectId, item => item with { Mesh = mesh, Material = material });
    }

    public SceneDocumentEditResult UpdateObjectTransform(
        SceneFileDocument document,
        string objectId,
        SceneFileTransformDefinition? transform)
    {
        return UpdateObject(document, objectId, item => item with { Transform = transform });
    }

    private static SceneDocumentEditResult UpdateObject(
        SceneFileDocument document,
        string objectId,
        Func<SceneFileObjectDefinition, SceneFileObjectDefinition> update,
        string? newObjectId = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(update);

        var objectIndex = FindObjectIndex(document, objectId);
        if (objectIndex < 0)
        {
            return Failure(
                SceneDocumentEditFailureKind.ObjectNotFound,
                $"Scene object id '{objectId}' was not found.");
        }

        if (!string.IsNullOrWhiteSpace(newObjectId) &&
            !string.Equals(objectId, newObjectId, StringComparison.Ordinal) &&
            ContainsObject(document, newObjectId))
        {
            return Failure(
                SceneDocumentEditFailureKind.DuplicateObjectId,
                $"Scene object id '{newObjectId}' is duplicated.");
        }

        var objects = document.Scene.Objects.ToArray();
        objects[objectIndex] = update(objects[objectIndex]);
        return ValidateAndReturn(WithObjects(document, objects));
    }

    private static SceneDocumentEditResult ValidateAndReturn(SceneFileDocument document)
    {
        var normalizeResult = SceneFileDocumentNormalizer.Normalize(document, "edited.scene.json");
        if (normalizeResult.IsSuccess)
        {
            return SceneDocumentEditResult.Success(document);
        }

        return Failure(MapFailureKind(normalizeResult.Failure!.Kind), normalizeResult.Failure.Message);
    }

    private static SceneDocumentEditFailureKind MapFailureKind(SceneDescriptionLoadFailureKind kind)
    {
        return kind switch
        {
            SceneDescriptionLoadFailureKind.DuplicateObjectId => SceneDocumentEditFailureKind.DuplicateObjectId,
            SceneDescriptionLoadFailureKind.MissingRequiredField => SceneDocumentEditFailureKind.MissingRequiredField,
            SceneDescriptionLoadFailureKind.InvalidReference => SceneDocumentEditFailureKind.InvalidReference,
            SceneDescriptionLoadFailureKind.InvalidValue => SceneDocumentEditFailureKind.InvalidTransform,
            _ => SceneDocumentEditFailureKind.InvalidDocument
        };
    }

    private static SceneFileDocument WithObjects(
        SceneFileDocument document,
        IReadOnlyList<SceneFileObjectDefinition> objects)
    {
        return document with
        {
            Scene = document.Scene with
            {
                Objects = objects
            }
        };
    }

    private static bool ContainsObject(SceneFileDocument document, string objectId)
    {
        return FindObjectIndex(document, objectId) >= 0;
    }

    private static int FindObjectIndex(SceneFileDocument document, string objectId)
    {
        for (var index = 0; index < document.Scene.Objects.Count; index += 1)
        {
            if (string.Equals(document.Scene.Objects[index].Id, objectId, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private static SceneDocumentEditResult Failure(SceneDocumentEditFailureKind kind, string message)
    {
        return SceneDocumentEditResult.FailureResult(new SceneDocumentEditFailure(kind, message));
    }
}
