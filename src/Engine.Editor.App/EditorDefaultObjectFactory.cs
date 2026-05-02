using System.Numerics;
using Engine.SceneData;

namespace Engine.Editor.App;

public sealed class EditorDefaultObjectFactory
{
    private const string kObjectIdPrefix = "object-";
    private const string kDefaultMesh = "mesh://cube";
    private const string kDefaultMaterial = "material://default";

    public SceneFileObjectDefinition Create(IReadOnlyList<EditorHierarchyItemSnapshot> existingObjects)
    {
        ArgumentNullException.ThrowIfNull(existingObjects);

        var objectId = CreateNextObjectId(existingObjects);
        return new SceneFileObjectDefinition(
            objectId,
            objectId,
            new SceneFileComponentDefinition[]
            {
                new SceneFileTransformComponentDefinition(
                    new SceneFileTransformDefinition(Vector3.Zero, Quaternion.Identity, Vector3.One)),
                new SceneFileMeshRendererComponentDefinition(kDefaultMesh, kDefaultMaterial)
            });
    }

    private static string CreateNextObjectId(IReadOnlyList<EditorHierarchyItemSnapshot> existingObjects)
    {
        var existingIds = existingObjects
            .Select(item => item.ObjectId)
            .ToHashSet(StringComparer.Ordinal);
        for (var index = 1; index < int.MaxValue; index += 1)
        {
            var candidate = $"{kObjectIdPrefix}{index:000}";
            if (!existingIds.Contains(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Could not allocate a default object id.");
    }
}
