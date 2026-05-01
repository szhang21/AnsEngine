namespace Engine.Scene;

public readonly record struct SceneUpdateContext
{
    public SceneUpdateContext(double deltaSeconds, double totalSeconds, bool anyInputDetected)
    {
        if (deltaSeconds < 0.0d)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "DeltaSeconds must not be negative.");
        }

        DeltaSeconds = deltaSeconds;
        TotalSeconds = totalSeconds;
        AnyInputDetected = anyInputDetected;
    }

    public double DeltaSeconds { get; }

    public double TotalSeconds { get; }

    public bool AnyInputDetected { get; }
}
