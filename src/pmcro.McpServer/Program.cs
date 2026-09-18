using AgentGovernance.Extensions.ModelContextProtocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pmcro.McpServer;
using Pmcro.McpServer.Tools;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddSingleton<GovernedPhaseAgentFactory>();
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<PmcroTools>()
    .WithGovernance(options =>
    {
        options.PolicyPaths.Add(Path.Combine(builder.Environment.ContentRootPath, "policies", "pmcro-governance.yaml"));
        options.DefaultAgentId = "did:mcp:pmcro-server";
        options.ServerName = "pmcro";
    });

await builder.Build().RunAsync();
