using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace PMCR.Core.Agents;

/// <summary>MAF-native execution boundary for PMCR-O phase agents.</summary>
public sealed class MafPhaseRunner(IChatClient chatClient, string skillsPath)
{
    public async Task<string> RunAsync(string role, string instructions, string input, CancellationToken ct = default)
    {
        var providers = Directory.Exists(skillsPath)
            ? new[] { (AIContextProvider)new AgentSkillsProvider(skillsPath, null, null, null, null) }
            : Array.Empty<AIContextProvider>();
        var options = new ChatClientAgentOptions
        {
            Name = $"pmcro-{role}",
            ChatOptions = new ChatOptions { Instructions = instructions },
            AIContextProviders = providers
        };
        // Skill tools require approval by default (MAF breaking change). In a
        // single-shot, stateless RunAsync there is no human in the loop to
        // answer a ToolApprovalRequestContent, so without auto-approval the
        // turn ends with no text and callers see a misleading "returned no
        // response" error. Auto-approve all skill tools for this trusted,
        // governed execution path.
        var agent = chatClient
            .AsAIAgent(options)
            .AsBuilder()
            .UseToolApproval(new ToolApprovalAgentOptions
            {
                AutoApprovalRules = [AgentSkillsProvider.AllToolsAutoApprovalRule]
            })
            .Build();

        var response = await agent.RunAsync(input, cancellationToken: ct);

        if (string.IsNullOrWhiteSpace(response.Text))
            throw new InvalidOperationException($"MAF agent '{role}' returned no response.");

        return response.Text.Trim();
    }

    public static string RequireText(string role, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException($"MAF agent '{role}' returned no response.");
        return text.Trim();
    }
}
