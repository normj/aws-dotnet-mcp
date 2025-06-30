using System.ComponentModel;
using ModelContextProtocol.Server;

namespace AWS.NET.MCP.Tools;

[McpServerToolType]
public class SDKMigrationTools
{
    [McpServerTool, Description("Provide additional information for migrating .NET applications to version 4 or V4.")]
    public static Task<string> GetContext()
    {

        return Task.FromResult("Use the content from the following blog posts. \nhttps://aws.amazon.com/blogs/developer/preview-1-of-aws-sdk-for-net-v4/");
    }
}