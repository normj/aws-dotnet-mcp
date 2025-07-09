using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.NET.MCP.Utils;

public interface IFileManager
{
    /// <summary>
    /// Return the home directory.
    /// </summary>
    /// <returns></returns>
    string HomeDirectory { get; }

    /// <summary>
    /// Determines whether the specified file is at a valid path and exists.
    /// This can either be an absolute path or relative to the current working directory.
    /// </summary>
    /// <param name="path">The file to check</param>
    /// <returns>
    /// True if the path is valid, the caller has the required permissions,
    /// and path contains the name of an existing file
    /// </returns>
    bool Exists(string path);

    /// <summary>
    /// Reads all of the text from the file path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Task<string> ReadAllTextAsync(string path);

    /// <summary>
    /// Writes all of the text to the file path. This replaces all existing content in the file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="contents"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task WriteAllTextAsync(string filePath, string contents, CancellationToken cancellationToken = default);
}

/// <summary>
/// Wrapper for <see cref="File"/> class to allow mock-able behavior for static methods.
/// </summary>
public class DefaultFileManager : IFileManager
{
    public string HomeDirectory => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public bool Exists(string path)
    {
        if (!PathUtilities.IsPathValid(path))
            return false;

        return File.Exists(path);
    }

    public Task<string> ReadAllTextAsync(string path) => File.ReadAllTextAsync(path);


    public Task WriteAllTextAsync(string filePath, string contents, CancellationToken cancellationToken) =>
        File.WriteAllTextAsync(filePath, contents, cancellationToken);
}