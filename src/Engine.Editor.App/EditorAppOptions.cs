namespace Engine.Editor.App;

public sealed class EditorAppOptions
{
    private const double kDefaultAutoExitSeconds = 0;
    private const string kEnableNativeImGuiFramesVariable = "ANS_ENGINE_EDITOR_ENABLE_NATIVE_IMGUI_FRAMES";

    public EditorAppOptions(double autoExitSeconds, bool enableNativeImGuiFrames)
    {
        AutoExitSeconds = autoExitSeconds;
        EnableNativeImGuiFrames = enableNativeImGuiFrames;
    }

    public double AutoExitSeconds { get; }

    public bool EnableNativeImGuiFrames { get; }

    public static EditorAppOptions FromEnvironment(IReadOnlyList<string> args)
    {
        var autoExitSeconds = ResolveAutoExitSeconds();
        if (args.Contains("--smoke-auto-exit", StringComparer.OrdinalIgnoreCase) && autoExitSeconds <= 0)
        {
            autoExitSeconds = 1.0;
        }

        return new EditorAppOptions(autoExitSeconds, ResolveEnableNativeImGuiFrames(args));
    }

    private static double ResolveAutoExitSeconds()
    {
        var value = Environment.GetEnvironmentVariable("ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS");
        if (string.IsNullOrWhiteSpace(value))
        {
            return kDefaultAutoExitSeconds;
        }

        return double.TryParse(value, out var seconds) && seconds > 0 ? seconds : kDefaultAutoExitSeconds;
    }

    private static bool ResolveEnableNativeImGuiFrames(IReadOnlyList<string> args)
    {
        if (args.Contains("--enable-native-imgui-frames", StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        var value = Environment.GetEnvironmentVariable(kEnableNativeImGuiFramesVariable);
        return !string.Equals(value, "0", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
    }
}
