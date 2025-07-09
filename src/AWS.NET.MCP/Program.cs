// See https://aka.ms/new-console-template for more information

using AWS.NET.MCP.Commands;
using AWS.NET.MCP.Utils;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var services = new ServiceCollection();

services.AddSingleton<IFileManager, DefaultFileManager>();

var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("AWS .NET MCP");
    config.AddCommand<RegisterCommand>("register")
        .WithDescription("Register mcp server with LLM tool.");
    config.AddCommand<UnregisterCommand>("unregister")
        .WithDescription("Unregister mcp server with LLM tool.");

    config.AddCommand<StartCommand>("start")
        .WithDescription("Start mcp server.");
});

await app.RunAsync(args);