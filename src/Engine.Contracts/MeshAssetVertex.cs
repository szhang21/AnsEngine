using System.Numerics;

namespace Engine.Contracts;

public readonly record struct MeshAssetVertex(
    Vector3 Position,
    Vector3 Normal,
    Vector2 TexCoord);
