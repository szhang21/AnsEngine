namespace Engine.SceneData;

public enum SceneDocumentStoreFailureKind
{
    None = 0,
    InvalidPath,
    NotFound,
    InvalidJson,
    ReadFailed,
    WriteFailed
}
