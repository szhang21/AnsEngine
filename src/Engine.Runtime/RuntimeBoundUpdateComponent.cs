namespace Engine.Runtime;

using Engine.Runtime.Abstractions;

internal sealed record RuntimeBoundUpdateComponent(
    IRuntimeUpdateComponent Component,
    IRuntimeObject Owner);
