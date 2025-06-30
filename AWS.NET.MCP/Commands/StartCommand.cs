using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Spectre.Console.Cli;

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
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        await builder.Build().RunAsync();        
    }
}