using AWS.NET.MCP.Commands;
using AWS.NET.MCP.Utils;
using ModelContextProtocol.Protocol;
using Spectre.Console.Cli;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AWS.NET.MCP.UnitTests
{
    public class RegisterTests
    {
        [Fact]
        public async Task RegisterAmazonQFileDoesNotExist()
        {
            var fileManager = new InMemoryFileManager();
            var registerCommand = new RegisterCommand(fileManager);

            registerCommand.Execute(null!, new RegisterCommand.Settings { Tool = ToolName.AmazonQ });

            Assert.True(fileManager.Exists(registerCommand.GetAmazonQConfigLocation()));

            var mcpJson = await fileManager.ReadAllTextAsync(registerCommand.GetAmazonQConfigLocation());
            using JsonDocument doc = JsonDocument.Parse(mcpJson);

            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("mcpServers", out var mcpServers));
            Assert.True(mcpServers.TryGetProperty(Constants.McpToolName, out var awsMcp));

            TaskValidateAmazonQMCPJson(awsMcp);
        }

        [Fact]
        public async Task RegisterAmazonQFileExistsWithOtherMcpServer()
        {
            var fileManager = new InMemoryFileManager();
            var registerCommand = new RegisterCommand(fileManager);

            var existingJson = """
                {
                    "mcpServers" : {
                        "ThirdParty" : {
                            "command": "run",
                            "args": ["execute"],
                            "env": {},
                            "timeout": 120000,
                            "disabled": false
                        }
                    }
                }
                """;

            await fileManager.WriteAllTextAsync(registerCommand.GetAmazonQConfigLocation(), existingJson);

            registerCommand.Execute(null!, new RegisterCommand.Settings { Tool = ToolName.AmazonQ });

            var mcpJson = await fileManager.ReadAllTextAsync(registerCommand.GetAmazonQConfigLocation());
            using JsonDocument doc = JsonDocument.Parse(mcpJson);

            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("mcpServers", out var mcpServers));
            Assert.Equal(2, mcpServers.EnumerateObject().Count());

            Assert.True(mcpServers.TryGetProperty(Constants.McpToolName, out var awsMcp));

            TaskValidateAmazonQMCPJson(awsMcp);
        }

        [Fact]
        public async Task RegisterAmazonQFailsDueToInvalidJson()
        {
            var fileManager = new InMemoryFileManager();
            var registerCommand = new RegisterCommand(fileManager);

            var existingInvalidJson = """
                {
                    }{
                }
                """;

            await fileManager.WriteAllTextAsync(registerCommand.GetAmazonQConfigLocation(), existingInvalidJson);

            Assert.Throws<InvalidOperationException>(() => registerCommand.Execute(null!, new RegisterCommand.Settings { Tool = ToolName.AmazonQ }));
        }

        [Fact]
        public async Task RegisterAmazonQFileUpdateAWSMcpServer()
        {
            var fileManager = new InMemoryFileManager();
            var registerCommand = new RegisterCommand(fileManager);

            var existingJson = """
                {
                    "mcpServers" : {
                        "{ToolName}" : {
                            "command": "run",
                            "args": ["execute"],
                            "env": {},
                            "timeout": 240000,
                            "disabled": false
                        }
                    }
                }
                """.Replace("{ToolName}", Constants.McpToolName);

            await fileManager.WriteAllTextAsync(registerCommand.GetAmazonQConfigLocation(), existingJson);

            registerCommand.Execute(null!, new RegisterCommand.Settings { Tool = ToolName.AmazonQ });

            var mcpJson = await fileManager.ReadAllTextAsync(registerCommand.GetAmazonQConfigLocation());
            using JsonDocument doc = JsonDocument.Parse(mcpJson);

            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("mcpServers", out var mcpServers));
            Assert.True(mcpServers.TryGetProperty(Constants.McpToolName, out var awsMcp));

            // Expect that the command args is updated but the timeout is left alone
            TaskValidateAmazonQMCPJson(awsMcp, 240000);
        }

        [Fact]
        public async Task UnregisterAmazonQWithExistingRegister()
        {
            var fileManager = new InMemoryFileManager();
            var registerCommand = new RegisterCommand(fileManager);
            var unregisterCommand = new UnregisterCommand(fileManager);

            registerCommand.Execute(null!, new RegisterCommand.Settings { Tool = ToolName.AmazonQ });

            Assert.True(fileManager.Exists(registerCommand.GetAmazonQConfigLocation()));

            unregisterCommand.Execute(null!, new UnregisterCommand.Settings { Tool = ToolName.AmazonQ });

            var mcpJson = await fileManager.ReadAllTextAsync(registerCommand.GetAmazonQConfigLocation());
            using JsonDocument doc = JsonDocument.Parse(mcpJson);
            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("mcpServers", out var mcpServers));
            Assert.Empty(mcpServers.EnumerateObject());
        }

        [Fact]
        public void UnregisterAmazonQFileDoesNotExist()
        {
            var fileManager = new InMemoryFileManager();
            var unregisterCommand = new UnregisterCommand(fileManager);

            unregisterCommand.Execute(null!, new UnregisterCommand.Settings { Tool = ToolName.AmazonQ });
            Assert.False(fileManager.Exists(unregisterCommand.GetAmazonQConfigLocation()));
        }

        [Fact]
        public async Task UnregisterAmazonQFileExistButEmpty()
        {
            var fileManager = new InMemoryFileManager();
            var unregisterCommand = new UnregisterCommand(fileManager);

            var existingJson = """
                {
                }
                """;

            await fileManager.WriteAllTextAsync(unregisterCommand.GetAmazonQConfigLocation(), existingJson);

            unregisterCommand.Execute(null!, new UnregisterCommand.Settings { Tool = ToolName.AmazonQ });

            var mcpJson = await fileManager.ReadAllTextAsync(unregisterCommand.GetAmazonQConfigLocation());
            using JsonDocument doc = JsonDocument.Parse(mcpJson);

            var root = doc.RootElement;

            Assert.False(root.TryGetProperty("mcpServers", out var _));
        }

        [Fact]
        public async Task UnregisterAmazonQFileExistButAmazonQDoesNotExist()
        {
            var fileManager = new InMemoryFileManager();
            var unregisterCommand = new UnregisterCommand(fileManager);

            var existingJson = """
                {
                    "mcpServers" : {
                        "ThirdParty" : {
                            "command": "run",
                            "args": ["execute"],
                            "env": {},
                            "timeout": 120000,
                            "disabled": false
                        }
                    }
                }
                """;

            await fileManager.WriteAllTextAsync(unregisterCommand.GetAmazonQConfigLocation(), existingJson);

            unregisterCommand.Execute(null!, new UnregisterCommand.Settings { Tool = ToolName.AmazonQ });

            var mcpJson = await fileManager.ReadAllTextAsync(unregisterCommand.GetAmazonQConfigLocation());
            using JsonDocument doc = JsonDocument.Parse(mcpJson);

            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("mcpServers", out var mcpServers));
            Assert.Single(mcpServers.EnumerateObject());
        }


        private void TaskValidateAmazonQMCPJson(JsonElement awsMcp, int expectedTimeout = 120000 )
        {
            Assert.True(awsMcp.TryGetProperty("command", out var command));
            Assert.Equal("dotnet", command.GetString());

            Assert.True(awsMcp.TryGetProperty("args", out var args));
            Assert.Equal(JsonValueKind.Array, args.ValueKind);
            Assert.Equal("aws-mcp", args[0].GetString());
            Assert.Equal("start", args[1].GetString());

            Assert.True(awsMcp.TryGetProperty("timeout", out var timeout));
            Assert.Equal(expectedTimeout, timeout.GetInt32());

            Assert.True(awsMcp.TryGetProperty("disabled", out var disabled));
            Assert.False(disabled.GetBoolean());
        }
    }
}