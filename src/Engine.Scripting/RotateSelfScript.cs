namespace Engine.Scripting;

using Engine.Runtime.Abstractions;
using System.Numerics;

public sealed class RotateSelfScript : IScriptBehavior, IRuntimeUpdateComponent
{
    public const string kScriptId = "RotateSelf";
    private const string kSpeedRadiansPerSecondPropertyName = "speedRadiansPerSecond";

    private double mSpeedRadiansPerSecond;

    public void Initialize(ScriptContext context)
    {
        mSpeedRadiansPerSecond = ReadSpeed(context);
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

        var rotationDelta = Quaternion.CreateFromAxisAngle(
            Vector3.UnitY,
            (float)(context.DeltaSeconds * mSpeedRadiansPerSecond));
        var transform = transformComponent.LocalTransform;
        transformComponent.SetLocalTransform(transform with
        {
            Rotation = Quaternion.Normalize(rotationDelta * transform.Rotation)
        });
        return RuntimeUpdateResult.Success();
    }

    private static double ReadSpeed(ScriptContext context)
    {
        return ScriptPropertyReader.RequireNumber(context, kSpeedRadiansPerSecondPropertyName);
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
