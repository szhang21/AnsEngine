using Engine.Contracts;
using System.Globalization;
using System.Numerics;

namespace Engine.Asset;

internal static class ObjMeshFileLoader
{
    public static MeshAssetData Load(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("OBJ path must not be null or whitespace.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("OBJ file was not found.", filePath);
        }

        var positions = new List<Vector3>();
        var normals = new List<Vector3>();
        var texCoords = new List<Vector2>();
        var vertices = new List<MeshAssetVertex>();
        var indices = new List<int>();
        var vertexMap = new Dictionary<ObjVertexKey, int>();
        var lines = File.ReadAllLines(filePath);

        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex += 1)
        {
            var line = lines[lineIndex].Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            switch (tokens[0])
            {
                case "v":
                    positions.Add(ParseVector3(tokens, lineIndex, "vertex position"));
                    break;
                case "vn":
                    normals.Add(ParseVector3(tokens, lineIndex, "vertex normal"));
                    break;
                case "vt":
                    texCoords.Add(ParseVector2(tokens, lineIndex, "vertex texcoord"));
                    break;
                case "f":
                    ParseFace(tokens, lineIndex, positions, normals, texCoords, vertexMap, vertices, indices);
                    break;
            }
        }

        if (vertices.Count == 0 || indices.Count == 0)
        {
            throw new InvalidDataException("OBJ file did not contain any indexed mesh faces.");
        }

        return new MeshAssetData(vertices, indices);
    }

    private static Vector3 ParseVector3(string[] tokens, int lineIndex, string kind)
    {
        if (tokens.Length < 4)
        {
            throw new InvalidDataException($"OBJ line {lineIndex + 1} must contain 3 float values for {kind}.");
        }

        return new Vector3(
            ParseFloat(tokens[1], lineIndex, kind),
            ParseFloat(tokens[2], lineIndex, kind),
            ParseFloat(tokens[3], lineIndex, kind));
    }

    private static Vector2 ParseVector2(string[] tokens, int lineIndex, string kind)
    {
        if (tokens.Length < 3)
        {
            throw new InvalidDataException($"OBJ line {lineIndex + 1} must contain 2 float values for {kind}.");
        }

        return new Vector2(
            ParseFloat(tokens[1], lineIndex, kind),
            ParseFloat(tokens[2], lineIndex, kind));
    }

    private static float ParseFloat(string token, int lineIndex, string kind)
    {
        if (!float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            throw new InvalidDataException($"OBJ line {lineIndex + 1} contains an invalid float for {kind}.");
        }

        return value;
    }

    private static void ParseFace(
        string[] tokens,
        int lineIndex,
        IReadOnlyList<Vector3> positions,
        IReadOnlyList<Vector3> normals,
        IReadOnlyList<Vector2> texCoords,
        IDictionary<ObjVertexKey, int> vertexMap,
        ICollection<MeshAssetVertex> vertices,
        ICollection<int> indices)
    {
        if (tokens.Length < 4)
        {
            throw new InvalidDataException($"OBJ line {lineIndex + 1} must define at least one triangle face.");
        }

        var faceIndices = new List<int>(tokens.Length - 1);
        for (var tokenIndex = 1; tokenIndex < tokens.Length; tokenIndex += 1)
        {
            var key = ParseFaceVertex(tokens[tokenIndex], lineIndex);
            if (!vertexMap.TryGetValue(key, out var vertexIndex))
            {
                var position = ResolvePosition(positions, key.PositionIndex, lineIndex);
                var texCoord = ResolveTexCoord(texCoords, key.TexCoordIndex, lineIndex);
                var normal = ResolveNormal(normals, key.NormalIndex, lineIndex);
                vertexIndex = vertices.Count;
                vertexMap[key] = vertexIndex;
                vertices.Add(new MeshAssetVertex(position, normal, texCoord));
            }

            faceIndices.Add(vertexIndex);
        }

        for (var triangleIndex = 1; triangleIndex < faceIndices.Count - 1; triangleIndex += 1)
        {
            indices.Add(faceIndices[0]);
            indices.Add(faceIndices[triangleIndex]);
            indices.Add(faceIndices[triangleIndex + 1]);
        }
    }

    private static ObjVertexKey ParseFaceVertex(string token, int lineIndex)
    {
        var parts = token.Split('/');
        if (parts.Length is < 1 or > 3 || string.IsNullOrWhiteSpace(parts[0]))
        {
            throw new InvalidDataException($"OBJ line {lineIndex + 1} contains an unsupported face vertex format.");
        }

        return new ObjVertexKey(
            ParseObjIndex(parts[0], lineIndex),
            ParseOptionalObjIndex(parts, 1, lineIndex),
            ParseOptionalObjIndex(parts, 2, lineIndex));
    }

    private static int ParseObjIndex(string token, int lineIndex)
    {
        if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) || index == 0)
        {
            throw new InvalidDataException($"OBJ line {lineIndex + 1} contains an invalid face index.");
        }

        return index;
    }

    private static int? ParseOptionalObjIndex(string[] parts, int partIndex, int lineIndex)
    {
        if (parts.Length <= partIndex || string.IsNullOrWhiteSpace(parts[partIndex]))
        {
            return null;
        }

        return ParseObjIndex(parts[partIndex], lineIndex);
    }

    private static Vector3 ResolvePosition(IReadOnlyList<Vector3> positions, int index, int lineIndex)
    {
        var resolvedIndex = ResolveListIndex(positions.Count, index, lineIndex, "position");
        return positions[resolvedIndex];
    }

    private static Vector3 ResolveNormal(IReadOnlyList<Vector3> normals, int? index, int lineIndex)
    {
        if (index is null)
        {
            return Vector3.Zero;
        }

        var resolvedIndex = ResolveListIndex(normals.Count, index.Value, lineIndex, "normal");
        return normals[resolvedIndex];
    }

    private static Vector2 ResolveTexCoord(IReadOnlyList<Vector2> texCoords, int? index, int lineIndex)
    {
        if (index is null)
        {
            return Vector2.Zero;
        }

        var resolvedIndex = ResolveListIndex(texCoords.Count, index.Value, lineIndex, "texcoord");
        return texCoords[resolvedIndex];
    }

    private static int ResolveListIndex(int count, int objIndex, int lineIndex, string kind)
    {
        var resolvedIndex = objIndex > 0 ? objIndex - 1 : count + objIndex;
        if ((uint)resolvedIndex >= (uint)count)
        {
            throw new InvalidDataException($"OBJ line {lineIndex + 1} references a missing {kind}.");
        }

        return resolvedIndex;
    }

    private readonly record struct ObjVertexKey(int PositionIndex, int? TexCoordIndex, int? NormalIndex);
}
