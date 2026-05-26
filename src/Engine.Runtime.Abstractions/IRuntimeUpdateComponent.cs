namespace Engine.Runtime.Abstractions;

public interface IRuntimeUpdateComponent : IRuntimeComponent
{
    RuntimeUpdateResult Update(RuntimeUpdateContext context);
}
