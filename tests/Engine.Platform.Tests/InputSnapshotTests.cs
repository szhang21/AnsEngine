using Engine.Platform;

namespace Engine.Platform.Tests;

public sealed class InputSnapshotTests
{
    [Fact]
    public void Empty_HasNoPressedKeysAndNoAnyInput()
    {
        var snapshot = InputSnapshot.Empty;

        Assert.False(snapshot.AnyInputDetected);
        Assert.False(snapshot.IsKeyDown(EngineKey.W));
        Assert.False(snapshot.IsKeyDown(EngineKey.A));
        Assert.False(snapshot.IsKeyDown(EngineKey.S));
        Assert.False(snapshot.IsKeyDown(EngineKey.D));
    }

    [Fact]
    public void FromKeys_SingleKey_ReportsOnlyThatKey()
    {
        var snapshot = InputSnapshot.FromKeys(EngineKey.W);

        Assert.True(snapshot.AnyInputDetected);
        Assert.True(snapshot.IsKeyDown(EngineKey.W));
        Assert.False(snapshot.IsKeyDown(EngineKey.A));
        Assert.False(snapshot.IsKeyDown(EngineKey.S));
        Assert.False(snapshot.IsKeyDown(EngineKey.D));
    }

    [Fact]
    public void FromKeys_MultipleKeys_ReportsEveryPressedKey()
    {
        var snapshot = InputSnapshot.FromKeys(EngineKey.W, EngineKey.D);

        Assert.True(snapshot.AnyInputDetected);
        Assert.True(snapshot.IsKeyDown(EngineKey.W));
        Assert.False(snapshot.IsKeyDown(EngineKey.A));
        Assert.False(snapshot.IsKeyDown(EngineKey.S));
        Assert.True(snapshot.IsKeyDown(EngineKey.D));
    }

    [Fact]
    public void FromKeys_NoKeys_MatchesEmptyInput()
    {
        var snapshot = InputSnapshot.FromKeys();

        Assert.Equal(InputSnapshot.Empty, snapshot);
        Assert.False(snapshot.AnyInputDetected);
    }

    [Fact]
    public void FromKeyStates_DerivesPressedKeysWithoutCollections()
    {
        var snapshot = InputSnapshot.FromKeyStates(
            isWDown: false,
            isADown: true,
            isSDown: false,
            isDDown: true);

        Assert.True(snapshot.AnyInputDetected);
        Assert.False(snapshot.IsKeyDown(EngineKey.W));
        Assert.True(snapshot.IsKeyDown(EngineKey.A));
        Assert.False(snapshot.IsKeyDown(EngineKey.S));
        Assert.True(snapshot.IsKeyDown(EngineKey.D));
    }

    [Fact]
    public void NullInputService_ReturnsEmptyInput()
    {
        var service = new NullInputService();

        var snapshot = service.GetSnapshot();

        Assert.Equal(InputSnapshot.Empty, snapshot);
        Assert.False(snapshot.AnyInputDetected);
    }

    [Fact]
    public void NativeWindowInputService_ReturnsEmptyInputWhenNoSupportedKeysArePressed()
    {
        var service = new NativeWindowInputService(new StubKeyboardStateProvider());

        var snapshot = service.GetSnapshot();

        Assert.Equal(InputSnapshot.Empty, snapshot);
        Assert.False(snapshot.AnyInputDetected);
    }

    [Fact]
    public void NativeWindowInputService_MapsPressedWasdKeysToInputSnapshot()
    {
        var service = new NativeWindowInputService(
            new StubKeyboardStateProvider(EngineKey.W, EngineKey.D));

        var snapshot = service.GetSnapshot();

        Assert.True(snapshot.AnyInputDetected);
        Assert.True(snapshot.IsKeyDown(EngineKey.W));
        Assert.False(snapshot.IsKeyDown(EngineKey.A));
        Assert.False(snapshot.IsKeyDown(EngineKey.S));
        Assert.True(snapshot.IsKeyDown(EngineKey.D));
    }

    [Fact]
    public void LegacyBooleanConstructor_DerivesAnyInputFromPressedKeys()
    {
        var empty = new InputSnapshot(false);
        var nonEmpty = new InputSnapshot(true);

        Assert.False(empty.AnyInputDetected);
        Assert.True(nonEmpty.AnyInputDetected);
        Assert.True(nonEmpty.IsKeyDown(EngineKey.W));
        Assert.True(nonEmpty.IsKeyDown(EngineKey.A));
        Assert.True(nonEmpty.IsKeyDown(EngineKey.S));
        Assert.True(nonEmpty.IsKeyDown(EngineKey.D));
    }

    private sealed class StubKeyboardStateProvider : IKeyboardStateProvider
    {
        private readonly HashSet<EngineKey> mPressedKeys;

        public StubKeyboardStateProvider(params EngineKey[] pressedKeys)
        {
            mPressedKeys = new HashSet<EngineKey>(pressedKeys);
        }

        public bool IsKeyDown(EngineKey key)
        {
            return mPressedKeys.Contains(key);
        }
    }
}
