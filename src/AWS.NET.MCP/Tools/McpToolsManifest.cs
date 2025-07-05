using System.Text.Json.Serialization;

public class McpToolsManifest
{
    [JsonPropertyName("memory-banks")]
    public Dictionary<string, MemoryBank> MemoryBanks { get; set; } = new();

    [JsonPropertyName("tools")]
    public Dictionary<string, Tool> Tools { get; set; } = new();
}

public class MemoryBank
{
    [JsonPropertyName("base-url")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("start-file")]
    public string StartFile { get; set; } = string.Empty;

    [JsonPropertyName("additional-files")]
    public List<string> AdditionalFiles { get; set; } = new();
}

public class Tool
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("memory-bank")]
    public string MemoryBank { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;
}