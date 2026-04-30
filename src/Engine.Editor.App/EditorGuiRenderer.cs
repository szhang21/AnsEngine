using ImGuiNET;
using System.Numerics;

namespace Engine.Editor.App;

public sealed class EditorGuiRenderer
{
    private readonly EditorAppController mController;
    private readonly EditorInspectorInputState mInspectorInputState = new();
    private readonly EditorFileWorkflowState mFileWorkflowState = new();
    private readonly EditorObjectWorkflowState mObjectWorkflowState = new();

    public EditorGuiRenderer(EditorAppController controller)
    {
        mController = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    public EditorGuiSnapshot? LastSnapshot { get; private set; }

    public void RenderFrame(bool enableNativeImGuiFrames)
    {
        var snapshot = EditorGuiSnapshotFactory.Create(mController);
        LastSnapshot = snapshot;
        if (!enableNativeImGuiFrames)
        {
            return;
        }

        DrawToolbar(snapshot);
        DrawHierarchy(snapshot);
        DrawInspector(snapshot);
        DrawStatusBar(snapshot);
    }

    private void DrawToolbar(EditorGuiSnapshot snapshot)
    {
        mFileWorkflowState.SyncFrom(snapshot);
        ImGui.Begin("Toolbar");
        var openPath = mFileWorkflowState.OpenPath;
        var saveAsPath = mFileWorkflowState.SaveAsPath;
        ImGui.InputText("Open Path", ref openPath, 512);
        mFileWorkflowState.SetOpenPath(openPath);
        if (ImGui.Button("Open"))
        {
            mFileWorkflowState.Open(mController);
        }

        ImGui.SameLine();
        if (ImGui.Button("Save"))
        {
            mFileWorkflowState.Save(mController);
        }

        ImGui.InputText("Save As Path", ref saveAsPath, 512);
        mFileWorkflowState.SetSaveAsPath(saveAsPath);
        if (ImGui.Button("Save As"))
        {
            mFileWorkflowState.SaveAs(mController);
        }

        ImGui.SameLine();
        if (ImGui.Button("Add Object"))
        {
            mObjectWorkflowState.AddObject(mController);
        }

        ImGui.SameLine();
        if (ImGui.Button("Remove Selected"))
        {
            mObjectWorkflowState.RemoveSelectedObject(mController);
        }

        ImGui.End();
    }

    private void DrawHierarchy(EditorGuiSnapshot snapshot)
    {
        ImGui.Begin("Hierarchy");
        if (snapshot.HierarchyItems.Count == 0)
        {
            ImGui.TextUnformatted("No objects.");
        }
        else
        {
            foreach (var item in snapshot.HierarchyItems)
            {
                if (ImGui.Selectable(item.DisplayName, item.IsSelected))
                {
                    mController.SelectObject(item.ObjectId);
                }
            }
        }

        ImGui.End();
    }

    private void DrawInspector(EditorGuiSnapshot snapshot)
    {
        ImGui.Begin("Inspector");
        if (!snapshot.Inspector.HasSelectedObject)
        {
            mInspectorInputState.SyncFrom(snapshot.Inspector);
            ImGui.TextUnformatted(snapshot.Inspector.EmptyMessage);
        }
        else
        {
            mInspectorInputState.SyncFrom(snapshot.Inspector);
            var objectId = mInspectorInputState.ObjectId;
            var objectName = mInspectorInputState.ObjectName;
            var mesh = mInspectorInputState.Mesh;
            var material = mInspectorInputState.Material;
            var position = mInspectorInputState.Position;
            var rotation = new Vector4(
                mInspectorInputState.Rotation.X,
                mInspectorInputState.Rotation.Y,
                mInspectorInputState.Rotation.Z,
                mInspectorInputState.Rotation.W);
            var scale = mInspectorInputState.Scale;

            ImGui.InputText("Id", ref objectId, 128);
            ImGui.InputText("Name", ref objectName, 128);
            ImGui.InputText("Mesh", ref mesh, 256);
            ImGui.InputText("Material", ref material, 256);
            ImGui.InputFloat3("Position", ref position);
            ImGui.InputFloat4("Rotation", ref rotation);
            ImGui.InputFloat3("Scale", ref scale);
            mInspectorInputState.SetTextValues(objectId, objectName, mesh, material);
            mInspectorInputState.SetTransformValues(
                position,
                new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W),
                scale);

            if (ImGui.Button("Apply"))
            {
                mInspectorInputState.Apply(mController, snapshot.Inspector);
            }
        }

        ImGui.End();
    }

    private static void DrawStatusBar(EditorGuiSnapshot snapshot)
    {
        ImGui.Begin("Status Bar");
        ImGui.TextUnformatted($"Scene: {snapshot.StatusBar.ScenePath}");
        ImGui.SameLine();
        ImGui.TextUnformatted($"Dirty: {snapshot.StatusBar.DirtyText}");
        ImGui.SameLine();
        ImGui.TextUnformatted($"Selected: {snapshot.StatusBar.SelectedObjectId}");
        if (!string.IsNullOrWhiteSpace(snapshot.StatusBar.LastError))
        {
            ImGui.TextUnformatted($"Last Error: {snapshot.StatusBar.LastError}");
        }

        ImGui.End();
    }
}
