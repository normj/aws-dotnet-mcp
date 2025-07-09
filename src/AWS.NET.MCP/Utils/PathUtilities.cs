using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.NET.MCP.Utils;

public static class PathUtilities
{
    public static bool IsPathValid(string path)
    {
        path = path.Trim();

        if (string.IsNullOrEmpty(path))
            return false;

        if (path.StartsWith(@"\\"))
            return false;

        if (path.Contains("&"))
            return false;

        if (Path.GetInvalidPathChars().Any(x => path.Contains(x)))
            return false;

        return true;
    }

    internal static string GetAmazonQConfigLocation(IFileManager fileManager)
    {
        return Path.Combine(fileManager.HomeDirectory, ".aws", "amazonq", "mcp.json");
    }
}