namespace Engine.SceneData;

public enum SceneDescriptionLoadFailureKind
{
    None = 0,
    NotFound,
    InvalidJson,
    MissingRequiredField,
    DuplicateObjectId,
    InvalidReference,
    InvalidValue
}
