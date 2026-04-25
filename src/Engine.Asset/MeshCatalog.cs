namespace Engine.Asset;

internal sealed class MeshCatalog
{
    private readonly IReadOnlyDictionary<string, string> mEntries;

    private MeshCatalog(IReadOnlyDictionary<string, string> entries)
    {
        mEntries = entries;
    }

    public static MeshCatalog Load(string catalogPath)
    {
        if (string.IsNullOrWhiteSpace(catalogPath))
        {
            throw new ArgumentException("Catalog path must not be null or whitespace.", nameof(catalogPath));
        }

        if (!File.Exists(catalogPath))
        {
            throw new FileNotFoundException("Mesh catalog file was not found.", catalogPath);
        }

        var lines = File.ReadAllLines(catalogPath);
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < lines.Length; index += 1)
        {
            var line = lines[index].Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0 || separatorIndex == line.Length - 1)
            {
                throw new InvalidDataException($"Catalog line {index + 1} must use '<meshId>=<relativePath>' format.");
            }

            var meshId = line[..separatorIndex].Trim();
            var relativePath = line[(separatorIndex + 1)..].Trim();
            if (string.IsNullOrWhiteSpace(meshId) || string.IsNullOrWhiteSpace(relativePath))
            {
                throw new InvalidDataException($"Catalog line {index + 1} must not contain empty mesh ids or paths.");
            }

            entries[meshId] = relativePath;
        }

        return new MeshCatalog(entries);
    }

    public bool TryResolve(string meshId, out string relativePath)
    {
        return mEntries.TryGetValue(meshId, out relativePath!);
    }
}
