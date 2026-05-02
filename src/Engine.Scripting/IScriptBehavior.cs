namespace Engine.Scripting;

public interface IScriptBehavior
{
    void Initialize(ScriptContext context);

    void Update(ScriptContext context);
}
