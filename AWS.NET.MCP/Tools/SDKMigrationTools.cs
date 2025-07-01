using System.ComponentModel;
using ModelContextProtocol.Server;

namespace AWS.NET.MCP.Tools;

[McpServerToolType]
public class SDKMigrationTools
{
    [McpServerTool, Description("Provide additional information for migrating .NET applications to version 4 or V4 of the AWS SDK for .NET.")]
    public static Task<string> ProvideAWSSDKMemoryBank()
    {
        return Task.FromResult(
            "In the local folder \"C:\\codebase\\v4\\aws-sdk-net-v4\\memory-bank\" is a memory bank or knowledge base describing the V4 of " + 
            "the AWS SDK for .NET architecture. Start from the start.md file to learn about the SDK to understand how to migrate to V4 of the AWS SDK for .NET.");
    }
}