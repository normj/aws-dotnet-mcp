using AWS.NET.MCP.Utils;
using ModelContextProtocol.Server;
using System.Text;
using System.Text.Json;

namespace AWS.NET.MCP.Tools;

public class ManifestToolsLoader
{
    private readonly string _manifestPath;

    public ManifestToolsLoader(string manifestPath)
    {
        _manifestPath = manifestPath;
    }

    public async Task<IEnumerable<McpServerTool>> GetManifestToolsAsync()
    {
        var json = await LoadManifestContent();
        var manifest = JsonSerializer.Deserialize<McpToolsManifest>(json, McpJsonContext.Default.McpToolsManifest);
        if (manifest == null)
            throw new InvalidOperationException("Invalid JSON for MCP json manifest");

        var tools = new List<McpServerTool>();

        foreach (var kvp  in manifest.Tools)
        {
            if (!manifest.MemoryBanks.TryGetValue(kvp.Value.MemoryBank, out var memoryBank))
                throw new InvalidOperationException($"The memory bank {kvp.Value.MemoryBank} referenced by the tool {kvp.Key} does not exist in the manifest list of memory banks");

            var toolWrapper = new ToolWrapper(kvp.Value.Prompt, memoryBank);

            tools.Add(McpServerTool.Create(toolWrapper.ExecuteAsync, new McpServerToolCreateOptions
            {
                Name = kvp.Key,
                Description = kvp.Value.Description
            }));
        }

        return tools;
    }

    private async Task<string> LoadManifestContent()
    {
        if (_manifestPath.StartsWith("http://") || _manifestPath.StartsWith("https://"))
        {
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetStringAsync(_manifestPath);
            }
        }

        if (!File.Exists(_manifestPath))
            throw new InvalidOperationException($"Path to tools manifest does not exist: {_manifestPath}");

        return File.ReadAllText(_manifestPath);
    }

    public class ToolWrapper
    {
        private const string MEMORY_BANK_DESCRIPTION =
            """
            Below is a memory bank or knowledge base formatted as a JSON document. The keys of the JSON document are the file names of the memory bank and the values.
            When reading the memory bank you must start from the {START_FILE} file which can be found by looking at the start.md key in the JSON document. 
            Each file in the JSON document representing the memory bank in markdown format. The markdown files will have references to other markdown files.
            Those files can be found as keys in the JSON document.
            """;

        private readonly string _prompt;
        private readonly MemoryBank _memoryBank;

        public ToolWrapper(string prompt, MemoryBank memoryBank) 
        {
            _prompt = prompt;
            _memoryBank = memoryBank;
        }

        public async Task<string> ExecuteAsync()
        {
            var memoryBankDefinition = await LoadMemoryBank();

            var builder = new StringBuilder();
            builder.AppendLine(MEMORY_BANK_DESCRIPTION.Replace("{START_FILE}", _memoryBank.StartFile));
            builder.AppendLine(_prompt);
            builder.AppendLine(memoryBankDefinition);

            return builder.ToString();
        }

        private async Task<string> LoadMemoryBank()
        {
            using var httpClient = new HttpClient();
            var files = new List<string>(_memoryBank.AdditionalFiles);
            files.Add(_memoryBank.StartFile);

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });

            writer.WriteStartObject();
            foreach (var file in files)
            {
                var uri = new Uri(new Uri(_memoryBank.BaseUrl), file);
                var content = await new HttpClient().GetStringAsync(uri);
                writer.WriteString(file, content);
            }
            writer.WriteEndObject();

            writer.Flush();

            string json = Encoding.UTF8.GetString(stream.ToArray());

            return json;
        }
    }
}
