using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace AWS.NET.MCP.Commands;

public class StartCommand : Command<StartCommand.Settings>
{
    public class Settings : CommandSettings
    {
        /// <summary>
        /// AWS credential profile used to make calls to AWS
        /// </summary>
        [CommandOption("--tools-manifest")]
        [Description("Overrides the default location of the manifest for mcp tools.")]
        public string? ToolsManifest { get; set; }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        StartMCPServer().GetAwaiter().GetResult();
        return 0;
    }

    private async Task StartMCPServer()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        await builder.Build().RunAsync();        
    }
}