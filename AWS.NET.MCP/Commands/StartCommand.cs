using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;
using AWS.NET.MCP.Tools;
using System.Reflection;

namespace AWS.NET.MCP.Commands;

public class StartCommand : Command<StartCommand.Settings>
{
    public class Settings : CommandSettings
    {

    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        StartMCPServer().GetAwaiter().GetResult();
        return 0;
    }

    private async Task StartMCPServer()
    {
        var manifestPath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location)!.FullName, "mcp-tools-manifest.json");
        var manifestToolsLoader = new ManifestToolsLoader(manifestPath);
        var manifestTools = await manifestToolsLoader.GetManifestToolsAsync();

        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            .WithTools(manifestTools)
            .WithToolsFromAssembly();

        await builder.Build().RunAsync();
    }
}