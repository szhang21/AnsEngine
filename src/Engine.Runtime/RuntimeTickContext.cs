namespace Engine.Runtime;

using Engine.Runtime.Abstractions;

public sealed record RuntimeTickContext(
    double DeltaSeconds,
    double TotalSeconds,
    RuntimeInputSnapshot Input);
