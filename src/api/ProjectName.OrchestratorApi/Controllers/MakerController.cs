using Microsoft.AspNetCore.Mvc;
using ProjectName.MakerGrpc;

namespace ProjectName.OrchestratorApi.Controllers;

/// <summary>
/// Thin HTTP facade over the stateless <see cref="Maker.MakerClient"/> gRPC
/// service. This is a single MAF call, not a persisted trail — see
/// ProjectName.MakerGrpc.Services.MakerGrpcService for what it actually does.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class MakerController(Maker.MakerClient client) : ControllerBase
{
    public sealed record MakerRunRequest(string Intent, string ApprovedPlan);
    public sealed record MakerRunResponse(string Result);

    [HttpPost("run")]
    public async Task<ActionResult<MakerRunResponse>> Run([FromBody] MakerRunRequest request, CancellationToken ct)
    {
        var reply = await client.RunCycleAsync(
            new Request { Intent = request.Intent, ApprovedPlan = request.ApprovedPlan },
            cancellationToken: ct);
        return Ok(new MakerRunResponse(reply.Result));
    }
}
