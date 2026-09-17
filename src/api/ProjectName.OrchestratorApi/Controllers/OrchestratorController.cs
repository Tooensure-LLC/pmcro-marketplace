using Microsoft.AspNetCore.Mvc;
using ProjectName.OrchestratorGrpc;

namespace ProjectName.OrchestratorApi.Controllers;

/// <summary>
/// Thin HTTP facade over the stateless <see cref="Orchestrator.OrchestratorClient"/>
/// gRPC service. NOTE: this is intent-routing triage only (see
/// ProjectName.OrchestratorGrpc.Services.OrchestratorGrpcService) — it does not
/// chain Planner/Maker/Checker/Reflector. There is currently no endpoint that runs
/// a full cycle; that lives in the separate, Postgres-backed ProjectName.Agents
/// workers, which have no HTTP or gRPC surface today.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class OrchestratorController(Orchestrator.OrchestratorClient client) : ControllerBase
{
    public sealed record OrchestratorRunRequest(string MessySeedIntent);
    public sealed record OrchestratorRunResponse(string TrueSeedIntent);

    [HttpPost("run")]
    public async Task<ActionResult<OrchestratorRunResponse>> Run([FromBody] OrchestratorRunRequest request, CancellationToken ct)
    {
        var reply = await client.RunAsync(
            new Request { MessySeedIntent = request.MessySeedIntent },
            cancellationToken: ct);
        return Ok(new OrchestratorRunResponse(reply.TrueSeedIntent));
    }
}
