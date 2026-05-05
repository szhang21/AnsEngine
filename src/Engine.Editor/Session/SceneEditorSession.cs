namespace Engine.Editor;

using Engine.SceneData;
using Engine.SceneData.Abstractions;

public sealed class SceneEditorSession
{
    private readonly ISceneDocumentStore mDocumentStore;
    private readonly SceneFileDocumentEditor mDocumentEditor;
    private SceneFileDocument? mDocument;
    private SceneDescription? mScene;

    public SceneEditorSession()
        : this(new JsonSceneDocumentStore(), new SceneFileDocumentEditor())
    {
    }

    public SceneEditorSession(ISceneDocumentStore documentStore)
        : this(documentStore, new SceneFileDocumentEditor())
    {
    }

    private SceneEditorSession(ISceneDocumentStore documentStore, SceneFileDocumentEditor documentEditor)
    {
        mDocumentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        mDocumentEditor = documentEditor ?? throw new ArgumentNullException(nameof(documentEditor));
    }

    public bool HasDocument => mDocument is not null;

    public string? SceneFilePath { get; private set; }

    public bool IsDirty { get; private set; }

    public string? SelectedObjectId { get; private set; }

    public SceneFileDocument? Document => mDocument;

    public SceneDescription? Scene => mScene;

    public IReadOnlyList<SceneObjectDescription> Objects => mScene?.Objects ?? Array.Empty<SceneObjectDescription>();

    public SceneObjectDescription? SelectedObject
    {
        get
        {
            if (SelectedObjectId is null)
            {
                return null;
            }

            return Objects.FirstOrDefault(item => string.Equals(item.ObjectId, SelectedObjectId, StringComparison.Ordinal));
        }
    }

    public SceneEditorSessionResult Open(string sceneFilePath)
    {
        var documentResult = mDocumentStore.Load(sceneFilePath);
        if (!documentResult.IsSuccess)
        {
            return SceneEditorSessionResult.FailureResult(
                new SceneEditorFailure(
                    SceneEditorFailureKind.OpenFailed,
                    documentResult.Failure!.Message,
                    documentResult.Failure.Path));
        }

        var normalizeResult = SceneFileDocumentNormalizer.Normalize(documentResult.Document!, sceneFilePath);
        if (!normalizeResult.IsSuccess)
        {
            return SceneEditorSessionResult.FailureResult(
                new SceneEditorFailure(
                    SceneEditorFailureKind.InvalidDocument,
                    normalizeResult.Failure!.Message,
                    normalizeResult.Failure.Path));
        }

        SceneFilePath = sceneFilePath;
        mDocument = documentResult.Document;
        mScene = normalizeResult.Scene;
        IsDirty = false;
        SelectedObjectId = null;
        return SceneEditorSessionResult.Success();
    }

    public SceneEditorSessionResult Close()
    {
        SceneFilePath = null;
        mDocument = null;
        mScene = null;
        IsDirty = false;
        SelectedObjectId = null;
        return SceneEditorSessionResult.Success();
    }

    public SceneEditorSessionResult SelectObject(string objectId)
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        if (FindObject(objectId) is null)
        {
            return SceneEditorSessionResult.FailureResult(
                new SceneEditorFailure(
                    SceneEditorFailureKind.ObjectNotFound,
                    $"Scene object id '{objectId}' was not found.",
                    objectId: objectId));
        }

        SelectedObjectId = objectId;
        return SceneEditorSessionResult.Success();
    }

    public SceneEditorSessionResult ClearSelection()
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        SelectedObjectId = null;
        return SceneEditorSessionResult.Success();
    }

    public SceneEditorSessionResult AddObject(SceneFileObjectDefinition objectDefinition)
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        var editResult = mDocumentEditor.AddObject(mDocument, objectDefinition);
        return ApplyEditResult(editResult);
    }

    public SceneEditorSessionResult AddObject(string objectId, string objectName)
    {
        var objectDefinition = new SceneFileObjectDefinition(
            objectId,
            objectName,
            new SceneFileComponentDefinition[]
            {
                new SceneFileTransformComponentDefinition(
                    new SceneFileTransformDefinition(null, null, null)),
                new SceneFileMeshRendererComponentDefinition("mesh://cube", "material://default")
            });
        return AddObject(objectDefinition);
    }

    public SceneEditorSessionResult RemoveObject(string objectId)
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        var editResult = mDocumentEditor.RemoveObject(mDocument, objectId);
        return ApplyEditResult(
            editResult,
            () =>
            {
                if (string.Equals(SelectedObjectId, objectId, StringComparison.Ordinal))
                {
                    SelectedObjectId = null;
                }
            });
    }

    public SceneEditorSessionResult RemoveSelectedObject()
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        if (SelectedObjectId is null)
        {
            return SceneEditorSessionResult.FailureResult(
                new SceneEditorFailure(
                    SceneEditorFailureKind.SelectionInvalid,
                    "No scene object is selected."));
        }

        return RemoveObject(SelectedObjectId);
    }

    public SceneEditorSessionResult UpdateObjectId(string currentObjectId, string newObjectId)
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        var editResult = mDocumentEditor.UpdateObjectId(mDocument, currentObjectId, newObjectId);
        return ApplyEditResult(
            editResult,
            () =>
            {
                if (string.Equals(SelectedObjectId, currentObjectId, StringComparison.Ordinal))
                {
                    SelectedObjectId = newObjectId;
                }
            });
    }

    public SceneEditorSessionResult UpdateObjectName(string objectId, string name)
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        var editResult = mDocumentEditor.UpdateObjectName(mDocument, objectId, name);
        return ApplyEditResult(editResult);
    }

    public SceneEditorSessionResult UpdateObjectResources(string objectId, string mesh, string? material)
    {
        return UpdateObjectMeshRendererComponent(
            objectId,
            new SceneFileMeshRendererComponentDefinition(mesh, material));
    }

    public SceneEditorSessionResult UpdateObjectTransform(string objectId, SceneFileTransformDefinition? transform)
    {
        return UpdateObjectTransformComponent(
            objectId,
            new SceneFileTransformComponentDefinition(transform));
    }

    public SceneEditorSessionResult UpdateObjectTransformComponent(
        string objectId,
        SceneFileTransformComponentDefinition transform)
    {
        ArgumentNullException.ThrowIfNull(transform);

        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        return UpdateObjectComponents(
            objectId,
            components => ReplaceComponent(components, transform));
    }

    public SceneEditorSessionResult UpdateObjectMeshRendererComponent(
        string objectId,
        SceneFileMeshRendererComponentDefinition meshRenderer)
    {
        ArgumentNullException.ThrowIfNull(meshRenderer);

        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        return UpdateObjectComponents(
            objectId,
            components => ReplaceComponent(components, meshRenderer));
    }

    public SceneEditorSessionResult RemoveObjectMeshRendererComponent(string objectId)
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        return UpdateObjectComponents(
            objectId,
            components => components
                .Where(item => !string.Equals(item.Type, SceneFileComponentTypes.MeshRenderer, StringComparison.Ordinal))
                .ToArray());
    }

    public SceneEditorSessionResult UpdateObjectScriptComponent(
        string objectId,
        SceneFileScriptComponentDefinition script)
    {
        ArgumentNullException.ThrowIfNull(script);

        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        return UpdateObjectComponents(
            objectId,
            components => ReplaceFirstMatchingScriptOrAppend(components, script));
    }

    public SceneEditorSessionResult RemoveObjectScriptComponent(string objectId, string scriptId)
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        return UpdateObjectComponents(
            objectId,
            components => RemoveFirstMatchingScript(components, scriptId));
    }

    public SceneEditorSessionResult UpdateObjectRigidBodyComponent(
        string objectId,
        SceneFileRigidBodyComponentDefinition rigidBody)
    {
        ArgumentNullException.ThrowIfNull(rigidBody);

        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        return UpdateObjectComponents(
            objectId,
            components => ReplaceComponent(components, rigidBody));
    }

    public SceneEditorSessionResult RemoveObjectRigidBodyComponent(string objectId)
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        return UpdateObjectComponents(
            objectId,
            components => components
                .Where(item => !string.Equals(item.Type, SceneFileComponentTypes.RigidBody, StringComparison.Ordinal))
                .ToArray());
    }

    public SceneEditorSessionResult UpdateObjectBoxColliderComponent(
        string objectId,
        SceneFileBoxColliderComponentDefinition boxCollider)
    {
        ArgumentNullException.ThrowIfNull(boxCollider);

        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        return UpdateObjectComponents(
            objectId,
            components => ReplaceComponent(components, boxCollider));
    }

    public SceneEditorSessionResult RemoveObjectBoxColliderComponent(string objectId)
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        return UpdateObjectComponents(
            objectId,
            components => components
                .Where(item => !string.Equals(item.Type, SceneFileComponentTypes.BoxCollider, StringComparison.Ordinal))
                .ToArray());
    }

    public SceneEditorSessionResult Save()
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        if (SceneFilePath is null)
        {
            return SceneEditorSessionResult.FailureResult(
                new SceneEditorFailure(
                    SceneEditorFailureKind.SaveFailed,
                    "No scene file path is associated with the open document."));
        }

        return SaveToPath(SceneFilePath);
    }

    public SceneEditorSessionResult SaveAs(string sceneFilePath)
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        return SaveToPath(sceneFilePath);
    }

    public SceneEditorSessionResult ReloadValidate()
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        var normalizeResult = SceneFileDocumentNormalizer.Normalize(mDocument, SceneFilePath ?? "<memory>");
        if (!normalizeResult.IsSuccess)
        {
            return SceneEditorSessionResult.FailureResult(
                new SceneEditorFailure(
                    SceneEditorFailureKind.ReloadValidationFailed,
                    normalizeResult.Failure!.Message,
                    normalizeResult.Failure.Path));
        }

        mScene = normalizeResult.Scene;
        return SceneEditorSessionResult.Success();
    }

    private SceneEditorSessionResult SaveToPath(string sceneFilePath)
    {
        var saveResult = mDocumentStore.Save(sceneFilePath, mDocument!);
        if (!saveResult.IsSuccess)
        {
            return SceneEditorSessionResult.FailureResult(
                new SceneEditorFailure(
                    SceneEditorFailureKind.SaveFailed,
                    saveResult.Failure!.Message,
                    saveResult.Failure.Path));
        }

        var reloadResult = mDocumentStore.Load(sceneFilePath);
        if (!reloadResult.IsSuccess)
        {
            return SceneEditorSessionResult.FailureResult(
                new SceneEditorFailure(
                    SceneEditorFailureKind.ReloadValidationFailed,
                    reloadResult.Failure!.Message,
                    reloadResult.Failure.Path));
        }

        var normalizeResult = SceneFileDocumentNormalizer.Normalize(reloadResult.Document!, sceneFilePath);
        if (!normalizeResult.IsSuccess)
        {
            return SceneEditorSessionResult.FailureResult(
                new SceneEditorFailure(
                    SceneEditorFailureKind.ReloadValidationFailed,
                    normalizeResult.Failure!.Message,
                    normalizeResult.Failure.Path));
        }

        SceneFilePath = sceneFilePath;
        mDocument = reloadResult.Document;
        mScene = normalizeResult.Scene;
        IsDirty = false;
        if (SelectedObjectId is not null && FindObject(SelectedObjectId) is null)
        {
            SelectedObjectId = null;
        }

        return SceneEditorSessionResult.Success();
    }

    private SceneObjectDescription? FindObject(string objectId)
    {
        return Objects.FirstOrDefault(item => string.Equals(item.ObjectId, objectId, StringComparison.Ordinal));
    }

    private SceneEditorSessionResult UpdateObjectComponents(
        string objectId,
        Func<IReadOnlyList<SceneFileComponentDefinition>, IReadOnlyList<SceneFileComponentDefinition>> update)
    {
        var objectIndex = FindDocumentObjectIndex(objectId);
        if (objectIndex < 0)
        {
            return SceneEditorSessionResult.FailureResult(
                new SceneEditorFailure(
                    SceneEditorFailureKind.ObjectNotFound,
                    $"Scene object id '{objectId}' was not found.",
                    objectId: objectId));
        }

        var objects = mDocument!.Scene.Objects.ToArray();
        var objectDefinition = objects[objectIndex];
        objects[objectIndex] = objectDefinition with
        {
            Components = update(objectDefinition.Components)
        };

        var updatedDocument = mDocument with
        {
            Scene = mDocument.Scene with
            {
                Objects = objects
            }
        };
        return ApplyDocumentCandidate(updatedDocument);
    }

    private int FindDocumentObjectIndex(string objectId)
    {
        if (mDocument is null)
        {
            return -1;
        }

        for (var index = 0; index < mDocument.Scene.Objects.Count; index += 1)
        {
            if (string.Equals(mDocument.Scene.Objects[index].Id, objectId, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private SceneEditorSessionResult ApplyEditResult(SceneDocumentEditResult editResult, Action? afterApply = null)
    {
        if (!editResult.IsSuccess)
        {
            return SceneEditorSessionResult.FailureResult(MapEditFailure(editResult.Failure!));
        }

        return ApplyDocumentCandidate(editResult.Document!, afterApply);
    }

    private SceneEditorSessionResult ApplyDocumentCandidate(SceneFileDocument document, Action? afterApply = null)
    {
        var normalizeResult = SceneFileDocumentNormalizer.Normalize(document, SceneFilePath ?? "<memory>");
        if (!normalizeResult.IsSuccess)
        {
            return SceneEditorSessionResult.FailureResult(
                new SceneEditorFailure(
                    MapNormalizeFailureKind(normalizeResult.Failure!.Kind),
                    normalizeResult.Failure!.Message,
                    normalizeResult.Failure.Path));
        }

        mDocument = document;
        mScene = normalizeResult.Scene;
        IsDirty = true;
        afterApply?.Invoke();
        return SceneEditorSessionResult.Success();
    }

    private static SceneEditorSessionResult CreateNoDocumentFailure()
    {
        return SceneEditorSessionResult.FailureResult(
            new SceneEditorFailure(
                SceneEditorFailureKind.NoDocumentOpen,
                "No scene document is open."));
    }

    private static SceneEditorFailure MapEditFailure(SceneDocumentEditFailure failure)
    {
        return new SceneEditorFailure(
            MapEditFailureKind(failure.Kind),
            failure.Message);
    }

    private static SceneEditorFailureKind MapEditFailureKind(SceneDocumentEditFailureKind kind)
    {
        return kind switch
        {
            SceneDocumentEditFailureKind.ObjectNotFound => SceneEditorFailureKind.ObjectNotFound,
            SceneDocumentEditFailureKind.DuplicateObjectId => SceneEditorFailureKind.DuplicateObjectId,
            SceneDocumentEditFailureKind.MissingRequiredField => SceneEditorFailureKind.InvalidReference,
            SceneDocumentEditFailureKind.InvalidReference => SceneEditorFailureKind.InvalidReference,
            SceneDocumentEditFailureKind.InvalidTransform => SceneEditorFailureKind.InvalidTransform,
            _ => SceneEditorFailureKind.InvalidDocument
        };
    }

    private static SceneEditorFailureKind MapNormalizeFailureKind(SceneDescriptionLoadFailureKind kind)
    {
        return kind switch
        {
            SceneDescriptionLoadFailureKind.DuplicateObjectId => SceneEditorFailureKind.DuplicateObjectId,
            SceneDescriptionLoadFailureKind.MissingRequiredField => SceneEditorFailureKind.InvalidReference,
            SceneDescriptionLoadFailureKind.InvalidReference => SceneEditorFailureKind.InvalidReference,
            SceneDescriptionLoadFailureKind.InvalidValue => SceneEditorFailureKind.InvalidTransform,
            _ => SceneEditorFailureKind.InvalidDocument
        };
    }

    private static IReadOnlyList<SceneFileComponentDefinition> ReplaceComponent(
        IReadOnlyList<SceneFileComponentDefinition> components,
        SceneFileComponentDefinition component)
    {
        return components
            .Where(item => !string.Equals(item.Type, component.Type, StringComparison.Ordinal))
            .Concat(new[] { component })
            .ToArray();
    }

    private static IReadOnlyList<SceneFileComponentDefinition> ReplaceFirstMatchingScriptOrAppend(
        IReadOnlyList<SceneFileComponentDefinition> components,
        SceneFileScriptComponentDefinition script)
    {
        var updated = components.ToArray();
        for (var index = 0; index < updated.Length; index += 1)
        {
            if (updated[index] is SceneFileScriptComponentDefinition existingScript &&
                string.Equals(existingScript.ScriptId, script.ScriptId, StringComparison.Ordinal))
            {
                updated[index] = script;
                return updated;
            }
        }

        return updated.Concat(new[] { script }).ToArray();
    }

    private static IReadOnlyList<SceneFileComponentDefinition> RemoveFirstMatchingScript(
        IReadOnlyList<SceneFileComponentDefinition> components,
        string scriptId)
    {
        var removed = false;
        return components
            .Where(
                item =>
                {
                    if (removed || item is not SceneFileScriptComponentDefinition script)
                    {
                        return true;
                    }

                    if (!string.Equals(script.ScriptId, scriptId, StringComparison.Ordinal))
                    {
                        return true;
                    }

                    removed = true;
                    return false;
                })
            .ToArray();
    }
}
