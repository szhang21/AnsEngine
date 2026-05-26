namespace Engine.Scripting;

using Engine.Runtime.Abstractions;
using System.Numerics;

public sealed class MoveOnInputScript : IScriptBehavior, IRuntimeUpdateComponent
{
    public const string kScriptId = "MoveOnInput";
    private const string kSpeedUnitsPerSecondPropertyName = "speedUnitsPerSecond";

    private double mSpeedUnitsPerSecond;

    public void Initialize(ScriptContext context)
    {
        mSpeedUnitsPerSecond = ReadSpeed(context);
    }

    public void Update(ScriptContext context)
    {
        var result = Update(
            new RuntimeUpdateContext(
                context.Self,
                context.DeltaSeconds,
                context.TotalSeconds,
                ConvertInput(context.Input)));
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.Failure!.Message);
        }
    }

    public RuntimeUpdateResult Update(RuntimeUpdateContext context)
    {
        var transformComponent = context.Owner.GetComponent<IRuntimeTransformComponent>();
        if (transformComponent is null)
        {
            return RuntimeUpdateResult.FailureResult(
                new RuntimeUpdateFailure(
                    $"Script id '{kScriptId}' on object '{context.Owner.ObjectId}' requires a Transform component.",
                    context.Owner.ObjectId,
                    kScriptId));
        }

        var direction = Vector3.Zero;
        if (context.Input.IsKeyDown(RuntimeKey.W))
        {
            direction += new Vector3(0.0f, 0.0f, -1.0f);
        }

        if (context.Input.IsKeyDown(RuntimeKey.S))
        {
            direction += new Vector3(0.0f, 0.0f, 1.0f);
        }

        if (context.Input.IsKeyDown(RuntimeKey.A))
        {
            direction += new Vector3(-1.0f, 0.0f, 0.0f);
        }

        if (context.Input.IsKeyDown(RuntimeKey.D))
        {
            direction += new Vector3(1.0f, 0.0f, 0.0f);
        }

        if (direction == Vector3.Zero)
        {
            return RuntimeUpdateResult.Success();
        }

        direction = Vector3.Normalize(direction);
        var transform = transformComponent.LocalTransform;
        transformComponent.SetLocalTransform(transform with
        {
            Position = transform.Position + (direction * (float)(mSpeedUnitsPerSecond * context.DeltaSeconds))
        });
        return RuntimeUpdateResult.Success();
    }

    private static double ReadSpeed(ScriptContext context)
    {
        return ScriptPropertyReader.RequireNumber(context, kSpeedUnitsPerSecondPropertyName);
    }

    private static RuntimeInputSnapshot ConvertInput(ScriptInputSnapshot input)
    {
        var keys = new List<RuntimeKey>(4);
        if (input.IsKeyDown(ScriptKey.W))
        {
            keys.Add(RuntimeKey.W);
        }

        if (input.IsKeyDown(ScriptKey.A))
        {
            keys.Add(RuntimeKey.A);
        }

        if (input.IsKeyDown(ScriptKey.S))
        {
            keys.Add(RuntimeKey.S);
        }

        if (input.IsKeyDown(ScriptKey.D))
        {
            keys.Add(RuntimeKey.D);
        }

        return keys.Count == 0 ? RuntimeInputSnapshot.Empty : RuntimeInputSnapshot.FromKeys(keys.ToArray());
    }
}
