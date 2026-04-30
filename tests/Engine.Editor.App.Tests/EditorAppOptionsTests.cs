using Engine.Editor.App;
using Xunit;

namespace Engine.Editor.App.Tests;

public sealed class EditorAppOptionsTests : IDisposable
{
    private const string kAutoExitSecondsVariable = "ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS";
    private const string kEnableNativeImGuiFramesVariable = "ANS_ENGINE_EDITOR_ENABLE_NATIVE_IMGUI_FRAMES";
    private readonly string? mOriginalAutoExitSeconds;
    private readonly string? mOriginalEnableNativeImGuiFrames;

    public EditorAppOptionsTests()
    {
        mOriginalAutoExitSeconds = Environment.GetEnvironmentVariable(kAutoExitSecondsVariable);
        mOriginalEnableNativeImGuiFrames = Environment.GetEnvironmentVariable(kEnableNativeImGuiFramesVariable);
        Environment.SetEnvironmentVariable(kAutoExitSecondsVariable, null);
        Environment.SetEnvironmentVariable(kEnableNativeImGuiFramesVariable, null);
    }

    [Fact]
    public void FromEnvironment_Defaults_EnablesNativeImGuiFrames()
    {
        var options = EditorAppOptions.FromEnvironment(Array.Empty<string>());

        Assert.Equal(0, options.AutoExitSeconds);
        Assert.True(options.EnableNativeImGuiFrames);
    }

    [Fact]
    public void FromEnvironment_ExplicitArgument_EnablesNativeImGuiFrames()
    {
        var options = EditorAppOptions.FromEnvironment(new[] { "--enable-native-imgui-frames" });

        Assert.True(options.EnableNativeImGuiFrames);
    }

    [Fact]
    public void FromEnvironment_EnvironmentVariable_EnablesNativeImGuiFrames()
    {
        Environment.SetEnvironmentVariable(kEnableNativeImGuiFramesVariable, "true");

        var options = EditorAppOptions.FromEnvironment(Array.Empty<string>());

        Assert.True(options.EnableNativeImGuiFrames);
    }

    [Fact]
    public void FromEnvironment_EnvironmentVariable_CanDisableNativeImGuiFramesForDiagnostics()
    {
        Environment.SetEnvironmentVariable(kEnableNativeImGuiFramesVariable, "false");

        var options = EditorAppOptions.FromEnvironment(Array.Empty<string>());

        Assert.False(options.EnableNativeImGuiFrames);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(kAutoExitSecondsVariable, mOriginalAutoExitSeconds);
        Environment.SetEnvironmentVariable(kEnableNativeImGuiFramesVariable, mOriginalEnableNativeImGuiFrames);
    }
}
