namespace Engine.SceneData;

public enum SceneDocumentEditFailureKind
{
    None = 0,
    ObjectNotFound,
    DuplicateObjectId,
    MissingRequiredField,
    InvalidReference,
    InvalidTransform,
    InvalidDocument
}
