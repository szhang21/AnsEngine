using Engine.Editor;
using Engine.SceneData;

namespace Engine.Editor.App;

public sealed class EditorAppController
{
    private readonly EditorScenePathResolver mScenePathResolver;

    public EditorAppController(EditorScenePathResolver scenePathResolver)
        : this(scenePathResolver, new SceneEditorSession())
    {
    }

    public EditorAppController(EditorScenePathResolver scenePathResolver, SceneEditorSession session)
    {
        mScenePathResolver = scenePathResolver ?? throw new ArgumentNullException(nameof(scenePathResolver));
        Session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public SceneEditorSession Session { get; }

    public string? LastError { get; private set; }

    public string StartupScenePath { get; private set; } = string.Empty;

    public bool OpenStartupScene()
    {
        try
        {
            StartupScenePath = mScenePathResolver.ResolveStartupScenePath();
            return OpenScene(StartupScenePath);
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
    }

    public bool OpenScene(string sceneFilePath)
    {
        var result = Session.Open(sceneFilePath);
        return CaptureResult(result);
    }

    public bool Save()
    {
        return CaptureResult(Session.Save());
    }

    public bool SaveAs(string sceneFilePath)
    {
        return CaptureResult(Session.SaveAs(sceneFilePath));
    }

    public bool SelectObject(string objectId)
    {
        return CaptureResult(Session.SelectObject(objectId));
    }

    public bool UpdateObjectName(string objectId, string name)
    {
        return CaptureResult(Session.UpdateObjectName(objectId, name));
    }

    public bool UpdateObjectId(string currentObjectId, string newObjectId)
    {
        return CaptureResult(Session.UpdateObjectId(currentObjectId, newObjectId));
    }

    public bool UpdateObjectResources(string objectId, string mesh, string? material)
    {
        return UpdateObjectMeshRendererComponent(objectId, mesh, material);
    }

    public bool UpdateObjectMeshRendererComponent(string objectId, string mesh, string? material)
    {
        return CaptureResult(
            Session.UpdateObjectMeshRendererComponent(
                objectId,
                new SceneFileMeshRendererComponentDefinition(mesh, material)));
    }

    public bool UpdateObjectTransform(string objectId, SceneFileTransformDefinition transform)
    {
        return UpdateObjectTransformComponent(objectId, transform);
    }

    public bool UpdateObjectTransformComponent(string objectId, SceneFileTransformDefinition transform)
    {
        return CaptureResult(
            Session.UpdateObjectTransformComponent(
                objectId,
                new SceneFileTransformComponentDefinition(transform)));
    }

    public bool RemoveObjectMeshRendererComponent(string objectId)
    {
        return CaptureResult(Session.RemoveObjectMeshRendererComponent(objectId));
    }

    public bool AddObject(SceneFileObjectDefinition objectDefinition)
    {
        return CaptureResult(Session.AddObject(objectDefinition));
    }

    public bool RemoveSelectedObject()
    {
        return CaptureResult(Session.RemoveSelectedObject());
    }

    public void SetLastError(string message)
    {
        LastError = message;
    }

    private bool CaptureResult(SceneEditorSessionResult result)
    {
        if (result.IsSuccess)
        {
            LastError = null;
            return true;
        }

        LastError = result.Failure?.Message ?? "Editor operation failed.";
        return false;
    }
}
