using Engine.SceneData.Abstractions;
using System.Text.Json;

namespace Engine.SceneData;

public sealed class JsonSceneDocumentStore : ISceneDocumentStore
{
    public SceneDocumentLoadResult Load(string sceneFilePath)
    {
        if (string.IsNullOrWhiteSpace(sceneFilePath))
        {
            return LoadFailure(
                SceneDocumentStoreFailureKind.InvalidPath,
                "Scene file path must not be null or whitespace.",
                sceneFilePath);
        }

        if (!File.Exists(sceneFilePath))
        {
            return LoadFailure(
                SceneDocumentStoreFailureKind.NotFound,
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
            return LoadFailure(
                SceneDocumentStoreFailureKind.ReadFailed,
                $"Scene file '{sceneFilePath}' could not be read: {ex.Message}",
                sceneFilePath);
        }

        SceneFileDocument? document;
        try
        {
            document = SceneFileJsonSerializer.Deserialize(json);
        }
        catch (JsonException ex)
        {
            return LoadFailure(
                SceneDocumentStoreFailureKind.InvalidJson,
                $"Scene file '{sceneFilePath}' contains invalid JSON: {ex.Message}",
                sceneFilePath,
                ex.LineNumber is null ? null : (int?)ex.LineNumber.Value);
        }

        if (document is null)
        {
            return LoadFailure(
                SceneDocumentStoreFailureKind.InvalidJson,
                $"Scene file '{sceneFilePath}' did not deserialize into a scene document.",
                sceneFilePath);
        }

        return SceneDocumentLoadResult.Success(document);
    }

    public SceneDocumentSaveResult Save(string sceneFilePath, SceneFileDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (string.IsNullOrWhiteSpace(sceneFilePath))
        {
            return SaveFailure(
                SceneDocumentStoreFailureKind.InvalidPath,
                "Scene file path must not be null or whitespace.",
                sceneFilePath);
        }

        var directoryPath = Path.GetDirectoryName(sceneFilePath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch (Exception ex)
            {
                return SaveFailure(
                    SceneDocumentStoreFailureKind.WriteFailed,
                    $"Scene file directory '{directoryPath}' could not be created: {ex.Message}",
                    sceneFilePath);
            }
        }

        try
        {
            var json = SceneFileJsonSerializer.Serialize(document);
            File.WriteAllText(sceneFilePath, json);
        }
        catch (Exception ex)
        {
            return SaveFailure(
                SceneDocumentStoreFailureKind.WriteFailed,
                $"Scene file '{sceneFilePath}' could not be written: {ex.Message}",
                sceneFilePath);
        }

        return SceneDocumentSaveResult.Success();
    }

    private static SceneDocumentLoadResult LoadFailure(
        SceneDocumentStoreFailureKind kind,
        string message,
        string path,
        int? lineNumber = null)
    {
        return SceneDocumentLoadResult.FailureResult(
            new SceneDocumentStoreFailure(kind, message, path, lineNumber));
    }

    private static SceneDocumentSaveResult SaveFailure(
        SceneDocumentStoreFailureKind kind,
        string message,
        string path)
    {
        return SceneDocumentSaveResult.FailureResult(
            new SceneDocumentStoreFailure(kind, message, path));
    }
}
