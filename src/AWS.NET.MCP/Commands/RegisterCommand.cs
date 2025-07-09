using AWS.NET.MCP.Utils;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AWS.NET.MCP.Commands;

public class RegisterCommand(IFileManager fileManager) : Command<RegisterCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[tool-name]")]
        [Description($"LLM tool to register the AWS .NET MCP server with. {Constants.VALID_TOOLNAMES_VALUES}")]
        [TypeConverter(typeof(CaseInsensitiveEnumConverter<ToolName>))]
        public ToolName? Tool { get; set; }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        if (settings.Tool == null)
        {
            AnsiConsole.MarkupLine($"[red]Tool name is required. {Constants.VALID_TOOLNAMES_VALUES}[/]");
            return 1;
        }

        switch (settings.Tool)
        {
            case ToolName.AmazonQ:
                RegisterWithAmazonQ().GetAwaiter().GetResult();
                break;
            default:
                AnsiConsole.MarkupLine($"[red]Unknown tool name: {settings.Tool}[/]");
                break;
        }

        return 0;
    }

    private async Task RegisterWithAmazonQ()
    {
        var mcpJsonPath = GetAmazonQConfigLocation();

        JsonNode? root;

        // 1. Create file if it doesn't exist
        if (!fileManager.Exists(mcpJsonPath))
        {
            root = new JsonObject
            {
                ["mcpServers"] = new JsonObject()
            };
        }
        else
        {
            string json = await fileManager.ReadAllTextAsync(mcpJsonPath);

            try
            {
                root = JsonNode.Parse(json);

                if (root == null)
                {
                    throw new InvalidOperationException($"Failed to register mcp server because the {mcpJsonPath} file is not valid JSON.");
                }
            }
            catch (JsonException e)
            {
                throw new InvalidOperationException($"Failed to register mcp server because the {mcpJsonPath} file is not valid JSON.", e);
            }
        }

        // 2. Ensure structure exists
        var mcpServers = root["mcpServers"] as JsonObject ?? new JsonObject();
        root["mcpServers"] = mcpServers;

        var awsMcp = mcpServers[Constants.McpToolName] as JsonObject ?? new JsonObject();
        mcpServers[Constants.McpToolName] = awsMcp;

        // 3. Update the command value
        awsMcp["command"] = "dotnet";
        awsMcp["args"] = JsonSerializer.Deserialize<JsonArray>(@"[ ""aws-mcp"", ""start"" ]");

        // 4. Ensure other default properties exist if they don't
        awsMcp["timeout"] ??= 120000;
        awsMcp["disabled"] ??= false;

        // 5. Write back to file with indentation
        var options = new JsonSerializerOptions { WriteIndented = true };

        AnsiConsole.MarkupLine($"Register in MCP config file [green]{mcpJsonPath}[/].");
        await fileManager.WriteAllTextAsync(mcpJsonPath, root.ToJsonString(options));
    }

    public string GetAmazonQConfigLocation()
    {
        return PathUtilities.GetAmazonQConfigLocation(fileManager);
    }
}