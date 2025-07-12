using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.NET.MCP.Tools;

[McpServerToolType]
public class FindSdkUsageTool
{
    [McpServerTool, Description("Find in a workspace all the files that use the AWS SDK for .NET.")]
    public static async Task<string> FindFilesUsingSdkAsync([Description("The path to workspace or solution file that should be searched.")] string workspace)
    {
        var msBuildWorkspace = MSBuildWorkspace.Create();
        var solution = await msBuildWorkspace.OpenSolutionAsync(@"C:\codebase\v3\aws-toolkit-visual-studio-staging\solutions\AWSVisualStudioToolkit.sln");

        var files = new HashSet<string>();

        var results = new StringBuilder();
        results.AppendLine("Files using the AWS SDK for .NET:");

        foreach (var project in solution.Projects)
        {
            var compilation = await project.GetCompilationAsync();
            if (compilation == null)
                continue;

            foreach (var document in project.Documents)
            {
                var tree = await document.GetSyntaxTreeAsync();
                if (tree == null)
                    continue;

                var semanticModel = compilation.GetSemanticModel(tree);

                var root = await tree.GetRootAsync();
                var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

                foreach (var invocation in invocations)
                {
                    var symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

                    if (symbol?.ContainingAssembly.ToDisplayString().StartsWith("AWSSDK") == true && document.FilePath != null)
                    {
                        if (!files.Contains(document.FilePath))
                        {
                            files.Add(document.FilePath);
                            results.AppendLine(document.FilePath);
                        }
                    }
                }
            }
        }

        File.WriteAllText(@"C:\temp\search.txt", results.ToString());
        return results.ToString();
    }
}
