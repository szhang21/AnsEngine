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
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        var editResult = mDocumentEditor.UpdateObjectResources(mDocument, objectId, mesh, material);
        return ApplyEditResult(editResult);
    }

    public SceneEditorSessionResult UpdateObjectTransform(string objectId, SceneFileTransformDefinition? transform)
    {
        if (mDocument is null)
        {
            return CreateNoDocumentFailure();
        }

        var editResult = mDocumentEditor.UpdateObjectTransform(mDocument, objectId, transform);
        return ApplyEditResult(editResult);
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

    private SceneEditorSessionResult ApplyEditResult(SceneDocumentEditResult editResult, Action? afterApply = null)
    {
        if (!editResult.IsSuccess)
        {
            return SceneEditorSessionResult.FailureResult(MapEditFailure(editResult.Failure!));
        }

        var normalizeResult = SceneFileDocumentNormalizer.Normalize(editResult.Document!, SceneFilePath ?? "<memory>");
        if (!normalizeResult.IsSuccess)
        {
            return SceneEditorSessionResult.FailureResult(
                new SceneEditorFailure(
                    SceneEditorFailureKind.ReloadValidationFailed,
                    normalizeResult.Failure!.Message,
                    normalizeResult.Failure.Path));
        }

        mDocument = editResult.Document;
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
}
