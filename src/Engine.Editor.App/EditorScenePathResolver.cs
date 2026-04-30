namespace Engine.Editor.App;

public sealed class EditorScenePathResolver
{
    private const string kScenePathEnvironmentVariableName = "ANS_ENGINE_EDITOR_SCENE_PATH";

    public string ResolveStartupScenePath()
    {
        var overridePath = Environment.GetEnvironmentVariable(kScenePathEnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(overridePath))
        {
            return Path.GetFullPath(overridePath);
        }

        var repositoryRoot = FindRepositoryRoot();
        return Path.Combine(repositoryRoot, "src", "Engine.App", "SampleScenes", "default.scene.json");
    }

    private static string FindRepositoryRoot()
    {
        foreach (var startDirectory in EnumerateSearchRoots())
        {
            var directory = new DirectoryInfo(startDirectory);
            while (directory is not null)
            {
                var solutionPath = Path.Combine(directory.FullName, "AnsEngine.sln");
                var sampleScenePath = Path.Combine(directory.FullName, "src", "Engine.App", "SampleScenes", "default.scene.json");
                if (File.Exists(solutionPath) && File.Exists(sampleScenePath))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing AnsEngine.sln and the source sample scene.");
    }

    private static IEnumerable<string> EnumerateSearchRoots()
    {
        yield return AppContext.BaseDirectory;
        yield return Directory.GetCurrentDirectory();
    }
}
