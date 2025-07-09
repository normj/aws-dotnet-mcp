using AWS.NET.MCP.Utils;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AWS.NET.MCP.Commands;

public class UnregisterCommand(IFileManager fileManager) : Command<UnregisterCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[tool-name]")]
        [Description($"LLM tool to unregister the AWS .NET MCP server with. {Constants.VALID_TOOLNAMES_VALUES}")]
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
                UnregisterWithAmazonQ().GetAwaiter().GetResult();
                break;
            default:
                AnsiConsole.MarkupLine($"[red]Unknown tool name: {settings.Tool}[/]");
                break;
        }

        return 0;
    }

    private async Task UnregisterWithAmazonQ()
    {
        var mcpJsonPath = GetAmazonQConfigLocation();
        AnsiConsole.MarkupLine($"Unregister in MCP config file [green]{mcpJsonPath}[/].");

        JsonNode? root;

        if (!fileManager.Exists(mcpJsonPath))
            return;

        string json = await fileManager.ReadAllTextAsync(mcpJsonPath);
        root = JsonNode.Parse(json);

        if (root == null)
            return;

        var mcpServers = root["mcpServers"] as JsonObject;
        if (mcpServers == null)
            return;

        if (!mcpServers.ContainsKey(Constants.McpToolName))
            return;

        mcpServers.Remove(Constants.McpToolName);

        var options = new JsonSerializerOptions { WriteIndented = true };
        await fileManager.WriteAllTextAsync(mcpJsonPath, root.ToJsonString(options));
    }

    public string GetAmazonQConfigLocation()
    {
        return PathUtilities.GetAmazonQConfigLocation(fileManager);
    }
}