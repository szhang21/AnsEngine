namespace Engine.Editor.App;

public static class EditorAppProgram
{
    public static int Run(string[] args)
    {
        var options = EditorAppOptions.FromEnvironment(args);
        var controller = new EditorAppController(new EditorScenePathResolver());
        controller.OpenStartupScene();

        try
        {
            using var window = new EditorAppWindow(controller, options);
            window.Run();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Editor GUI failed: {ex.Message}");
            return 1;
        }
    }
}
