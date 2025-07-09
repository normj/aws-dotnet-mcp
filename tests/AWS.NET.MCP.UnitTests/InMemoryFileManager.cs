using AWS.NET.MCP.Utils;
using System.Collections.Concurrent;

namespace AWS.NET.MCP.UnitTests;

public class InMemoryFileManager : IFileManager
{
    // Simulated file system using a concurrent dictionary
    private readonly ConcurrentDictionary<string, string> _fileStorage = new(StringComparer.OrdinalIgnoreCase);

    public string HomeDirectory => "Home";

    public bool Exists(string path)
    {
        return _fileStorage.ContainsKey(path);
    }

    public Task<string> ReadAllTextAsync(string path)
    {
        if (!_fileStorage.TryGetValue(path, out var content))
        {
            throw new FileNotFoundException($"File not found at path: {path}");
        }

        return Task.FromResult(content);
    }

    public Task WriteAllTextAsync(string filePath, string contents, CancellationToken cancellationToken = default)
    {
        _fileStorage[filePath] = contents;
        return Task.CompletedTask;
    }
}
