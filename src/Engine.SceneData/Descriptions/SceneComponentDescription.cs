using Engine.Contracts;
using System.Numerics;

namespace Engine.SceneData;

public abstract record SceneComponentDescription(string Type);

public sealed record SceneTransformComponentDescription(SceneTransformDescription Transform)
    : SceneComponentDescription(SceneComponentDescriptionTypes.Transform);

public sealed record SceneMeshRendererComponentDescription(SceneMeshRef Mesh, SceneMaterialRef Material)
    : SceneComponentDescription(SceneComponentDescriptionTypes.MeshRenderer);

public sealed record SceneScriptComponentDescription(
    string ScriptId,
    IReadOnlyDictionary<string, SceneScriptPropertyValue> Properties)
    : SceneComponentDescription(SceneComponentDescriptionTypes.Script);

public sealed record SceneRigidBodyComponentDescription(
    SceneRigidBodyType BodyType,
    double Mass)
    : SceneComponentDescription(SceneComponentDescriptionTypes.RigidBody);

public sealed record SceneBoxColliderComponentDescription(
    Vector3 Size,
    Vector3 Center)
    : SceneComponentDescription(SceneComponentDescriptionTypes.BoxCollider);

public enum SceneRigidBodyType
{
    Static,
    Dynamic
}

public readonly record struct SceneScriptPropertyValue
{
    private SceneScriptPropertyValue(double? number, bool? boolean, string? text)
    {
        Number = number;
        Boolean = boolean;
        Text = text;
    }

    public double? Number { get; }

    public bool? Boolean { get; }

    public string? Text { get; }

    public bool IsNumber => Number is not null;

    public bool IsBoolean => Boolean is not null;

    public bool IsString => Text is not null;

    public static SceneScriptPropertyValue FromNumber(double value)
    {
        return new SceneScriptPropertyValue(value, null, null);
    }

    public static SceneScriptPropertyValue FromBoolean(bool value)
    {
        return new SceneScriptPropertyValue(null, value, null);
    }

    public static SceneScriptPropertyValue FromString(string value)
    {
        return new SceneScriptPropertyValue(null, null, value ?? string.Empty);
    }
}

public static class SceneComponentDescriptionTypes
{
    public const string Transform = "Transform";
    public const string MeshRenderer = "MeshRenderer";
    public const string Script = "Script";
    public const string RigidBody = "RigidBody";
    public const string BoxCollider = "BoxCollider";
}
