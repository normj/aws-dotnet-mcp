using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AWS.NET.MCP.Commands;

public class RegisterCommand : Command<RegisterCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[name]")]
        [Description("Name of the user or entity to register.")]
        public string? Name { get; set; }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        AnsiConsole.MarkupLine($"[green]Registering:[/] {settings.Name ?? "No name provided"}");
        // TODO: Implement actual registration logic.
        return 0;
    }
}