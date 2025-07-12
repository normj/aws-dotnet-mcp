using System.Text.Json.Serialization;

namespace AWS.NET.MCP.Utils;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(McpToolsManifest))]
[JsonSerializable(typeof(MemoryBankManifest))]
internal partial class McpJsonContext : JsonSerializerContext
{
}
