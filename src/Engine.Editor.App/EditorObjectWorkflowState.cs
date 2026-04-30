namespace Engine.Editor.App;

public sealed class EditorObjectWorkflowState
{
    private readonly EditorDefaultObjectFactory mDefaultObjectFactory = new();

    public bool AddObject(EditorAppController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (!controller.Session.HasDocument)
        {
            controller.SetLastError("No scene document is open.");
            return false;
        }

        var snapshot = EditorGuiSnapshotFactory.Create(controller);
        var objectDefinition = mDefaultObjectFactory.Create(snapshot.HierarchyItems);
        if (!controller.AddObject(objectDefinition))
        {
            return false;
        }

        return controller.SelectObject(objectDefinition.Id);
    }

    public bool RemoveSelectedObject(EditorAppController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        if (controller.Session.SelectedObjectId is null)
        {
            controller.SetLastError("No scene object is selected.");
            return false;
        }

        return controller.RemoveSelectedObject();
    }
}
