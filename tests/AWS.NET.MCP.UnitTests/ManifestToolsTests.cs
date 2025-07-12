using AWS.NET.MCP.Tools;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Moq;
using System.Reflection;
using System.Text.Json;

namespace AWS.NET.MCP.UnitTests;

public class ManifestToolsTests
{
    [Fact]
    public async Task LoadMigrationTool()
    {
        Mock mockMcpServer = new Mock<IMcpServer>();
        IMcpServer? mcpServer = mockMcpServer.Object as IMcpServer;
        if (mcpServer == null)
            Assert.Fail("Failed to create MCP server mock");

        var loader = new ManifestToolsLoader(FindMcpToolsManifest());
        var tools = (await loader.GetManifestToolsAsync()).ToList();

        var migrationTool = tools.FirstOrDefault(x => string.Equals(x.ProtocolTool.Name, "ProvideAWSSDKMemoryBank"));
        Assert.NotNull(migrationTool);

        var toolOutput = await migrationTool.InvokeAsync(new RequestContext<CallToolRequestParams>(mcpServer));
        Assert.NotNull(toolOutput);

        var memoryBankDefinition = FindMemoryBankFromToolOutput(toolOutput).RootElement.EnumerateObject().ToDictionary(prop => prop.Name);
        Assert.True(memoryBankDefinition.Count > 0);

        // There should be 6 core files and 3 partition files.
        Assert.Equal(10, memoryBankDefinition.Count);

        // Ensure core files were loaded
        Assert.Contains("start.md", memoryBankDefinition);
        Assert.Equal(7, memoryBankDefinition.Keys.Where(x => !x.Contains("/")).Count());

        // Ensure loaded partition files
        Assert.Equal(3, memoryBankDefinition.Keys.Where(x => x.Contains("partitions/v3v4-upgrade")).Count());
    }

    private JsonDocument FindMemoryBankFromToolOutput(CallToolResult callToolResult)
    {
        var textContentblock = callToolResult.Content.First() as TextContentBlock;
        Assert.NotNull(textContentblock);

        var lines = textContentblock.Text.Split('\n');
        var jsonLine = lines.FirstOrDefault(line => line.StartsWith('{'));
        Assert.NotNull(jsonLine);

        return JsonDocument.Parse(jsonLine);
    }

    private string FindMcpToolsManifest()
    {
        var currentDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location);

        while (currentDirectory != null)
        {
            if (currentDirectory.GetDirectories().FirstOrDefault(x => x.Name == "src") != null)
            {
                return Path.Combine(currentDirectory.FullName, "src", "AWS.NET.MCP", "mcp-tools-manifest.json");
            }

            currentDirectory = currentDirectory.Parent;
        }

        Assert.Fail("Failed to find mcp-tools-manifest.json");
        return string.Empty;
    }
}
