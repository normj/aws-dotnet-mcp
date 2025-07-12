dotnet tool uninstall -g AWS.NET.MCP
dotnet pack
dotnet tool install -g AWS.NET.MCP --source .\bin\Release\
dotnet aws-mcp register AmazonQ