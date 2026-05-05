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
        var displaySize = enableNativeImGuiFrames ? ImGui.GetIO().DisplaySize : new Vector2(1100.0f, 720.0f);
        var snapshot = EditorGuiSnapshotFactory.Create(mController, displaySize);
        LastSnapshot = snapshot;
        if (!enableNativeImGuiFrames)
        {
            return;
        }

        ApplyTheme(snapshot.Theme);
        DrawToolbar(snapshot);
        DrawHierarchy(snapshot);
        DrawSceneView(snapshot);
        DrawInspector(snapshot);
        DrawStatusBar(snapshot);
    }

    private void DrawToolbar(EditorGuiSnapshot snapshot)
    {
        mFileWorkflowState.SyncFrom(snapshot);
        SetDockedPanelBounds(snapshot.Layout.ToolbarPosition, snapshot.Layout.ToolbarSize);
        ImGui.Begin("Toolbar", DockedPanelFlags());
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
        SetDockedPanelBounds(snapshot.Layout.HierarchyPosition, snapshot.Layout.HierarchySize);
        ImGui.Begin("Hierarchy", DockedPanelFlags());
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

    private static void DrawSceneView(EditorGuiSnapshot snapshot)
    {
        SetDockedPanelBounds(snapshot.Layout.SceneViewPosition, snapshot.Layout.SceneViewSize);
        ImGui.Begin("Scene View", DockedPanelFlags());
        ImGui.TextUnformatted("Scene View");
        ImGui.Separator();
        ImGui.BeginChild("SceneViewSurface", Vector2.Zero, ImGuiChildFlags.Border);
        DrawScenePreview(snapshot.ScenePreview);
        ImGui.EndChild();
        ImGui.End();
    }

    private static void DrawScenePreview(EditorScenePreviewSnapshot preview)
    {
        var origin = ImGui.GetCursorScreenPos();
        var size = ImGui.GetContentRegionAvail();
        size = new Vector2(Math.Max(1.0f, size.X), Math.Max(1.0f, size.Y));
        var drawList = ImGui.GetWindowDrawList();
        var background = preview.IsNonBlank
            ? ImGui.GetColorU32(new Vector4(0.10f, 0.13f, 0.15f, 1.0f))
            : ImGui.GetColorU32(new Vector4(0.08f, 0.08f, 0.08f, 1.0f));
        drawList.AddRectFilled(origin, origin + size, background);

        if (preview.IsNonBlank)
        {
            var center = origin + size * 0.5f;
            var half = MathF.Min(size.X, size.Y) * 0.16f;
            var color = ImGui.GetColorU32(new Vector4(0.42f, 0.85f, 0.58f, 1.0f));
            drawList.AddTriangleFilled(
                new Vector2(center.X, center.Y - half),
                new Vector2(center.X - half, center.Y + half),
                new Vector2(center.X + half, center.Y + half),
                color);
        }

        ImGui.TextUnformatted(preview.StatusText);
        ImGui.TextUnformatted($"Items: {preview.RenderItemCount}  Batches: {preview.BatchCount}  Vertices: {preview.MeshVertexCount}");
    }

    private void DrawInspector(EditorGuiSnapshot snapshot)
    {
        SetDockedPanelBounds(snapshot.Layout.InspectorPosition, snapshot.Layout.InspectorSize);
        ImGui.Begin("Inspector", DockedPanelFlags());
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
            var scriptId = mInspectorInputState.FirstScriptId;
            var scriptPropertiesJson = mInspectorInputState.FirstScriptPropertiesJson;
            var rigidBodyType = mInspectorInputState.RigidBodyType;
            var rigidBodyMass = mInspectorInputState.RigidBodyMass;
            var boxColliderSize = mInspectorInputState.BoxColliderSize;
            var boxColliderCenter = mInspectorInputState.BoxColliderCenter;
            var position = mInspectorInputState.Position;
            var rotation = new Vector4(
                mInspectorInputState.Rotation.X,
                mInspectorInputState.Rotation.Y,
                mInspectorInputState.Rotation.Z,
                mInspectorInputState.Rotation.W);
            var scale = mInspectorInputState.Scale;

            ImGui.TextUnformatted(snapshot.Inspector.Object.Title);
            ImGui.InputText("Id", ref objectId, 128);
            ImGui.InputText("Name", ref objectName, 128);
            ImGui.Separator();

            ImGui.TextUnformatted(snapshot.Inspector.Transform.Title);
            ImGui.InputFloat3("Position", ref position);
            ImGui.InputFloat4("Rotation", ref rotation);
            ImGui.InputFloat3("Scale", ref scale);
            ImGui.Separator();

            ImGui.TextUnformatted(snapshot.Inspector.MeshRenderer.Title);
            if (snapshot.Inspector.MeshRenderer.HasMeshRenderer)
            {
                ImGui.InputText("Mesh", ref mesh, 256);
                ImGui.InputText("Material", ref material, 256);
            }
            else
            {
                ImGui.TextUnformatted(snapshot.Inspector.MeshRenderer.EmptyMessage);
            }

            ImGui.Separator();
            ImGui.TextUnformatted(snapshot.Inspector.Scripts.Title);
            ImGui.SameLine();
            if (ImGui.SmallButton("+ Script"))
            {
                mInspectorInputState.AddScriptComponent(mController, snapshot.Inspector);
            }

            if (snapshot.Inspector.Scripts.Scripts.Count == 0)
            {
                ImGui.TextUnformatted(snapshot.Inspector.Scripts.EmptyMessage);
            }
            else
            {
                ImGui.InputText("Script Id", ref scriptId, 128);
                ImGui.InputTextMultiline("Properties", ref scriptPropertiesJson, 2048, new Vector2(-1.0f, 96.0f));
                if (ImGui.SmallButton("- Script"))
                {
                    mInspectorInputState.RemoveFirstScriptComponent(mController, snapshot.Inspector);
                }
            }

            ImGui.Separator();
            ImGui.TextUnformatted(snapshot.Inspector.RigidBody.Title);
            ImGui.SameLine();
            if (snapshot.Inspector.RigidBody.HasRigidBody)
            {
                if (ImGui.SmallButton("- RigidBody"))
                {
                    mInspectorInputState.RemoveRigidBodyComponent(mController, snapshot.Inspector);
                }

                var bodyTypes = new[] { "Static", "Dynamic" };
                var bodyTypeIndex = string.Equals(rigidBodyType, "Static", StringComparison.Ordinal) ? 0 : 1;
                if (ImGui.Combo("Body Type", ref bodyTypeIndex, bodyTypes, bodyTypes.Length))
                {
                    rigidBodyType = bodyTypes[bodyTypeIndex];
                }

                ImGui.InputDouble("Mass", ref rigidBodyMass);
            }
            else
            {
                if (ImGui.SmallButton("+ RigidBody"))
                {
                    mInspectorInputState.AddRigidBodyComponent(mController, snapshot.Inspector);
                }

                ImGui.TextUnformatted(snapshot.Inspector.RigidBody.EmptyMessage);
            }

            ImGui.Separator();
            ImGui.TextUnformatted(snapshot.Inspector.BoxCollider.Title);
            ImGui.SameLine();
            if (snapshot.Inspector.BoxCollider.HasBoxCollider)
            {
                if (ImGui.SmallButton("- BoxCollider"))
                {
                    mInspectorInputState.RemoveBoxColliderComponent(mController, snapshot.Inspector);
                }

                ImGui.InputFloat3("Collider Size", ref boxColliderSize);
                ImGui.InputFloat3("Collider Center", ref boxColliderCenter);
            }
            else
            {
                if (ImGui.SmallButton("+ BoxCollider"))
                {
                    mInspectorInputState.AddBoxColliderComponent(mController, snapshot.Inspector);
                }

                ImGui.TextUnformatted(snapshot.Inspector.BoxCollider.EmptyMessage);
            }

            ImGui.Separator();
            ImGui.TextUnformatted(snapshot.Inspector.PhysicsParticipation.Title);
            ImGui.TextUnformatted(snapshot.Inspector.PhysicsParticipation.IsPhysicsReady ? "Ready" : "Missing required components");

            mInspectorInputState.SetTextValues(objectId, objectName, mesh, material);
            if (snapshot.Inspector.Scripts.Scripts.Count > 0)
            {
                mInspectorInputState.SetScriptValues(scriptId, scriptPropertiesJson);
            }
            else
            {
                mInspectorInputState.ClearScript();
            }

            if (snapshot.Inspector.RigidBody.HasRigidBody)
            {
                mInspectorInputState.SetRigidBodyValues(rigidBodyType, rigidBodyMass);
            }
            else
            {
                mInspectorInputState.ClearRigidBody();
            }

            if (snapshot.Inspector.BoxCollider.HasBoxCollider)
            {
                mInspectorInputState.SetBoxColliderValues(boxColliderSize, boxColliderCenter);
            }
            else
            {
                mInspectorInputState.ClearBoxCollider();
            }

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
        SetDockedPanelBounds(snapshot.Layout.StatusBarPosition, snapshot.Layout.StatusBarSize);
        ImGui.Begin("Status Bar", DockedPanelFlags());
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

    private static void SetDockedPanelBounds(Vector2 position, Vector2 size)
    {
        ImGui.SetNextWindowPos(position, ImGuiCond.Always);
        ImGui.SetNextWindowSize(size, ImGuiCond.Always);
    }

    private static void ApplyTheme(EditorGuiThemeSnapshot theme)
    {
        var style = ImGui.GetStyle();
        style.WindowRounding = theme.WindowRounding;
        style.FrameRounding = theme.FrameRounding;
        style.ChildRounding = theme.ChildRounding;
        style.FramePadding = theme.FramePadding;
        style.ItemSpacing = theme.ItemSpacing;
        style.WindowBorderSize = 1.0f;
        style.ChildBorderSize = 1.0f;

        var colors = style.Colors;
        colors[(int)ImGuiCol.WindowBg] = theme.WindowBackground;
        colors[(int)ImGuiCol.ChildBg] = theme.PanelBackground;
        colors[(int)ImGuiCol.Header] = theme.Header;
        colors[(int)ImGuiCol.HeaderHovered] = theme.HeaderHovered;
        colors[(int)ImGuiCol.HeaderActive] = theme.HeaderActive;
        colors[(int)ImGuiCol.Border] = theme.Border;
        colors[(int)ImGuiCol.Button] = theme.Header;
        colors[(int)ImGuiCol.ButtonHovered] = theme.HeaderHovered;
        colors[(int)ImGuiCol.ButtonActive] = theme.HeaderActive;
        colors[(int)ImGuiCol.FrameBg] = new Vector4(0.16f, 0.17f, 0.18f, 1.0f);
        colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.20f, 0.22f, 0.24f, 1.0f);
        colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.24f, 0.27f, 0.30f, 1.0f);
    }

    private static ImGuiWindowFlags DockedPanelFlags()
    {
        return ImGuiWindowFlags.NoMove |
               ImGuiWindowFlags.NoResize |
               ImGuiWindowFlags.NoCollapse |
               ImGuiWindowFlags.NoSavedSettings;
    }
}
