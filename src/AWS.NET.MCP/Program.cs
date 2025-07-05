// See https://aka.ms/new-console-template for more information

using AWS.NET.MCP.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("AWS .NET MCP");
    config.AddCommand<RegisterCommand>("register")
        .WithDescription("Register mcp server with LLM.");

    config.AddCommand<StartCommand>("start")
        .WithDescription("Start mcp server.");
});

await app.RunAsync(args);