using Engine.Core;
using Engine.Platform;
using Engine.Render;
using Xunit;

namespace Engine.Render.Tests;

public sealed class NullRendererTests
{
    [Fact]
    public void Constructor_MissingSceneProvider_ThrowsArgumentNullException()
    {
        var windowService = new TestWindowService();
        var runtimeInfo = new EngineRuntimeInfo("AnsEngine", "0.1.0");

        _ = Assert.Throws<ArgumentNullException>(() => new NullRenderer(windowService, runtimeInfo, sceneProvider: null!));
    }

    private sealed class TestWindowService : IWindowService
    {
        public WindowConfig Configuration { get; } = new(1280, 720, "RenderTests");
        public bool IsCloseRequested => false;
        public bool Exists => true;
        public void ProcessEvents(double timeoutSeconds = 0) { }
        public void Present() { }
        public void RequestClose() { }
        public void Dispose() { }
    }
}
