using AWS.NET.MCP.Utils;
using ModelContextProtocol.Server;
using System.Collections.Concurrent;
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

            var toolWrapper = new ToolWrapper(kvp.Key, kvp.Value.Prompt, memoryBank, kvp.Value.Partitions);

            tools.Add(McpServerTool.Create(toolWrapper.Execute, new McpServerToolCreateOptions
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

        private static readonly ConcurrentDictionary<Uri, string> _cachedMemoryBankFiles = new ConcurrentDictionary<Uri, string>();

        private readonly string _toolName;
        private readonly string _prompt;
        private readonly ToolMemoryBankIdentifier _toolMemoryBankIdentifier;
        private readonly IList<string> _partitions;

        public ToolWrapper(string toolName, string prompt, ToolMemoryBankIdentifier toolMemoryBankIdentifier, IList<string> partitions) 
        {
            _toolName = toolName;
            _prompt = prompt;
            _toolMemoryBankIdentifier = toolMemoryBankIdentifier;
            _partitions = partitions;
        }

        public string Execute()
        {
            try
            {
                var memoryBankManifest = LoadMemoryBankManifest();
                var memoryBankDefinition = LoadMemoryBank(memoryBankManifest);

                var builder = new StringBuilder();
                builder.AppendLine(MEMORY_BANK_DESCRIPTION.Replace("{START_FILE}", memoryBankManifest.StartFile));
                builder.AppendLine(_prompt);
                builder.AppendLine(memoryBankDefinition);

                return builder.ToString();
            }
            catch (Exception ex)
            {
                // User stderr to not interfere with the MCP communication of stdout.
                Console.Error.WriteLine($"Failed to build prompt for tool {_toolName}:");
                Console.Error.WriteLine(ex.ToString());

                return string.Empty;
            }
        }

        private MemoryBankManifest LoadMemoryBankManifest()
        {
            var uri = new Uri(new Uri(_toolMemoryBankIdentifier.BaseUrl), _toolMemoryBankIdentifier.Manifest);
            var json = LoadFile(uri);
            var manifest = JsonSerializer.Deserialize<MemoryBankManifest>(json, McpJsonContext.Default.MemoryBankManifest);
            if (manifest == null)
                throw new InvalidOperationException("Invalid JSON for Memory Bank json manifest");

            return manifest;
        }

        private string LoadMemoryBank(MemoryBankManifest memoryBankManifest)
        {
            var files = new List<string>(memoryBankManifest.CoreFiles)
            {
                memoryBankManifest.StartFile
            };

            foreach (var partition in memoryBankManifest.Partitions.Where(x => _partitions.Contains(x.Name)))
            {
                foreach (var file in partition.Files)
                {
                    files.Add($"partitions/{partition.Name}/{file}");
                }
            }

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });

            writer.WriteStartObject();
            foreach (var file in files)
            {
                var uri = new Uri(new Uri(_toolMemoryBankIdentifier.BaseUrl), file);
                var content = LoadFile(uri);
                writer.WriteString(file, content);
            }
            writer.WriteEndObject();

            writer.Flush();

            string json = Encoding.UTF8.GetString(stream.ToArray());

            return json;
        }


        private static string LoadFile(Uri uri)
        {
            var content = _cachedMemoryBankFiles.GetOrAdd(uri, (uri) =>
            {
                using var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Referrer = new Uri("https://aws.net.mcp/");

                using var response = httpClient.Send(request);
                response.EnsureSuccessStatusCode();

                using var stream = response.Content.ReadAsStream();
                return new StreamReader(stream).ReadToEnd();
            });

            return content;
        }
    }
}
