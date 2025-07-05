using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using AWS.NET.MCP.Tools;

namespace AWS.NET.MCP.Commands;

public class StartCommand : Command<StartCommand.Settings>
{
    public class Settings : CommandSettings
    {
        /// <summary>
        /// AWS credential profile used to make calls to AWS
        /// </summary>
        [CommandOption("--tools-manifest")]
        [Description("Overrides the default location of the manifest for mcp tools. This is primary used for development purposes of AWS.NET.MCP.")]
        public string? ToolsManifest { get; set; }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        StartMCPServer(settings).GetAwaiter().GetResult();
        return 0;
    }

    private async Task StartMCPServer(Settings settings)
    {
        var manifestPath = DetermineManifestLocation(settings);
        var manifestToolsLoader = new ManifestToolsLoader(manifestPath);
        var manifestTools = await manifestToolsLoader.GetManifestToolsAsync();

        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            .WithTools(manifestTools)
            .WithToolsFromAssembly();

        await builder.Build().RunAsync();
    }

    private string DetermineManifestLocation(Settings settings)
    {
        if (!string.IsNullOrEmpty(settings.ToolsManifest))
        {
            string fullPath;
            if (Path.IsPathRooted(settings.ToolsManifest))
                fullPath = settings.ToolsManifest;
            else
                fullPath = Path.Combine(Environment.CurrentDirectory, settings.ToolsManifest);

            if (!File.Exists(fullPath))
                throw new InvalidOperationException($"Path to tools manifest does not exist: {fullPath}");

            return fullPath;
        }

        return Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location)!.FullName, "mcp-tools-manifest.json");
    }
}