using Microsoft.AspNetCore.Mvc;
using ProjectName.PlannerGrpc;

namespace ProjectName.OrchestratorApi.Controllers;

/// <summary>
/// Thin HTTP facade over the stateless <see cref="Planner.PlannerClient"/> gRPC
/// service. This is a single MAF call, not a persisted trail — see
/// ProjectName.PlannerGrpc.Services.PlannerGrpcService for what it actually does.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class PlannerController(Planner.PlannerClient client) : ControllerBase
{
    public sealed record PlannerRunRequest(string MessySeedIntent);
    public sealed record PlannerRunResponse(string TrueSeedIntent);

    [HttpPost("run")]
    public async Task<ActionResult<PlannerRunResponse>> Run([FromBody] PlannerRunRequest request, CancellationToken ct)
    {
        var reply = await client.RunCycleAsync(
            new Request { MessySeedIntent = request.MessySeedIntent },
            cancellationToken: ct);
        return Ok(new PlannerRunResponse(reply.TrueSeedIntent));
    }
}
