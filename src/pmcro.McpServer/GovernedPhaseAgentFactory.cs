using AgentGovernance;
using AgentGovernance.Extensions.Microsoft.Agents;
using AgentGovernance.Policy;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using OllamaSharp;

namespace Pmcro.McpServer;

public sealed class GovernedPhaseAgentFactory(IHostEnvironment environment)
{
    private const string DefaultEndpoint = "http://localhost:11434";
    private const string DefaultModel = "qwen3:8b";

    public AIAgent Create(string role, string instructions)
    {
        var endpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? DefaultEndpoint;
        var model = Environment.GetEnvironmentVariable("OLLAMA_MODEL_NAME") ?? DefaultModel;
        var policyPath = Path.Combine(environment.ContentRootPath, "policies", "pmcro-governance.yaml");

        var kernel = new GovernanceKernel(new GovernanceOptions
        {
            PolicyPaths = [policyPath],
            ConflictStrategy = ConflictResolutionStrategy.DenyOverrides,
            EnableRings = true,
            EnablePromptInjectionDetection = true,
            EnableCircuitBreaker = true
        });

        IChatClient chatClient = new OllamaApiClient(new Uri(endpoint), model);
        AIAgent agent = chatClient.AsAIAgent(instructions: instructions, name: $"pmcro-{role}");

        return agent.WithGovernance(kernel, new AgentFrameworkGovernanceOptions
        {
            DefaultAgentId = $"did:agentmesh:pmcro-{role}",
            EnableFunctionMiddleware = false
        });
    }
}
