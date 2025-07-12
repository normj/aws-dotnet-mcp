using System.Text.Json.Serialization;

public class McpToolsManifest
{
    [JsonPropertyName("memory-banks")]
    public Dictionary<string, ToolMemoryBankIdentifier> MemoryBanks { get; set; } = new();

    [JsonPropertyName("tools")]
    public Dictionary<string, Tool> Tools { get; set; } = new();
}

public class ToolMemoryBankIdentifier
{
    [JsonPropertyName("base-url")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("manifest")]
    public string Manifest { get; set; } = string.Empty;
}

public class Tool
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("memory-bank")]
    public string MemoryBank { get; set; } = string.Empty;

    [JsonPropertyName("partitions")]
    public List<string> Partitions { get; set; } = new ();

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;
}