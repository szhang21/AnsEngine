namespace Engine.Editor;

public enum SceneEditorFailureKind
{
    None = 0,
    NoDocumentOpen,
    OpenFailed,
    SaveFailed,
    ReloadValidationFailed,
    ObjectNotFound,
    DuplicateObjectId,
    InvalidReference,
    InvalidTransform,
    InvalidDocument,
    SelectionInvalid
}
