namespace Engine.Runtime.Abstractions;

public sealed record RuntimeUpdateContext(
    IRuntimeObject Owner,
    double DeltaSeconds,
    double TotalSeconds,
    RuntimeInputSnapshot Input);
