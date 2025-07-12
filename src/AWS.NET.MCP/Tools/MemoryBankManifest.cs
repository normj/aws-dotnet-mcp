using System.Text.Json.Serialization;

public class MemoryBankManifest
{
    [JsonPropertyName("start-file")]
    public string StartFile { get; set; } = string.Empty;

    [JsonPropertyName("core-files")]
    public List<string> CoreFiles { get; set; } = new List<string>();

    [JsonPropertyName("partitions")]
    public List<Partition> Partitions { get; set; } = new List<Partition>();
}

public class Partition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("files")]
    public List<string> Files { get; set; } = new List<string>();
}