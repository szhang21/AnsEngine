using Engine.Editor;
using Engine.SceneData;

namespace Engine.Editor.App;

public sealed class EditorAppController
{
    private readonly EditorScenePathResolver mScenePathResolver;
    private readonly EditorScenePreviewHost mPreviewHost;

    public EditorAppController(EditorScenePathResolver scenePathResolver)
        : this(scenePathResolver, new SceneEditorSession(), EditorScenePreviewHost.CreateDefault())
    {
    }

    public EditorAppController(EditorScenePathResolver scenePathResolver, SceneEditorSession session)
        : this(scenePathResolver, session, EditorScenePreviewHost.CreateDefault())
    {
    }

    internal EditorAppController(
        EditorScenePathResolver scenePathResolver,
        SceneEditorSession session,
        EditorScenePreviewHost previewHost)
    {
        mScenePathResolver = scenePathResolver ?? throw new ArgumentNullException(nameof(scenePathResolver));
        Session = session ?? throw new ArgumentNullException(nameof(session));
        mPreviewHost = previewHost ?? throw new ArgumentNullException(nameof(previewHost));
    }

    public SceneEditorSession Session { get; }

    public EditorScenePreviewSnapshot PreviewSnapshot => mPreviewHost.Snapshot;

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
        return CaptureResultAndRefreshPreview(result);
    }

    public bool Save()
    {
        return CaptureResultAndRefreshPreview(Session.Save());
    }

    public bool SaveAs(string sceneFilePath)
    {
        return CaptureResultAndRefreshPreview(Session.SaveAs(sceneFilePath));
    }

    public bool SelectObject(string objectId)
    {
        return CaptureResultAndRefreshPreview(Session.SelectObject(objectId));
    }

    public bool UpdateObjectName(string objectId, string name)
    {
        return CaptureResultAndRefreshPreview(Session.UpdateObjectName(objectId, name));
    }

    public bool UpdateObjectId(string currentObjectId, string newObjectId)
    {
        return CaptureResultAndRefreshPreview(Session.UpdateObjectId(currentObjectId, newObjectId));
    }

    public bool UpdateObjectResources(string objectId, string mesh, string? material)
    {
        return UpdateObjectMeshRendererComponent(objectId, mesh, material);
    }

    public bool UpdateObjectMeshRendererComponent(string objectId, string mesh, string? material)
    {
        return CaptureResultAndRefreshPreview(
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
        return CaptureResultAndRefreshPreview(
            Session.UpdateObjectTransformComponent(
                objectId,
                new SceneFileTransformComponentDefinition(transform)));
    }

    public bool RemoveObjectMeshRendererComponent(string objectId)
    {
        return CaptureResultAndRefreshPreview(Session.RemoveObjectMeshRendererComponent(objectId));
    }

    public bool UpdateObjectScriptComponent(
        string objectId,
        SceneFileScriptComponentDefinition script)
    {
        return CaptureResultAndRefreshPreview(Session.UpdateObjectScriptComponent(objectId, script));
    }

    public bool RemoveObjectScriptComponent(string objectId, string scriptId)
    {
        return CaptureResultAndRefreshPreview(Session.RemoveObjectScriptComponent(objectId, scriptId));
    }

    public bool UpdateObjectRigidBodyComponent(
        string objectId,
        SceneFileRigidBodyComponentDefinition rigidBody)
    {
        return CaptureResultAndRefreshPreview(Session.UpdateObjectRigidBodyComponent(objectId, rigidBody));
    }

    public bool RemoveObjectRigidBodyComponent(string objectId)
    {
        return CaptureResultAndRefreshPreview(Session.RemoveObjectRigidBodyComponent(objectId));
    }

    public bool UpdateObjectBoxColliderComponent(
        string objectId,
        SceneFileBoxColliderComponentDefinition boxCollider)
    {
        return CaptureResultAndRefreshPreview(Session.UpdateObjectBoxColliderComponent(objectId, boxCollider));
    }

    public bool RemoveObjectBoxColliderComponent(string objectId)
    {
        return CaptureResultAndRefreshPreview(Session.RemoveObjectBoxColliderComponent(objectId));
    }

    public bool AddObject(SceneFileObjectDefinition objectDefinition)
    {
        return CaptureResultAndRefreshPreview(Session.AddObject(objectDefinition));
    }

    public bool RemoveSelectedObject()
    {
        return CaptureResultAndRefreshPreview(Session.RemoveSelectedObject());
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

    private bool CaptureResultAndRefreshPreview(SceneEditorSessionResult result)
    {
        var success = CaptureResult(result);
        if (success)
        {
            mPreviewHost.Refresh(Session.Scene);
        }

        return success;
    }
}
