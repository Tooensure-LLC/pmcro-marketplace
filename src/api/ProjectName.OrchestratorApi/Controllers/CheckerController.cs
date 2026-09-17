using Microsoft.AspNetCore.Mvc;
using ProjectName.CheckerGrpc;

namespace ProjectName.OrchestratorApi.Controllers;

/// <summary>
/// Thin HTTP facade over the stateless <see cref="Checker.CheckerClient"/> gRPC
/// service. This is a single MAF call, not a persisted trail — see
/// ProjectName.CheckerGrpc.Services.CheckerGrpcService for what it actually does.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class CheckerController(Checker.CheckerClient client) : ControllerBase
{
    public sealed record CheckerRunRequest(string Intent, string Evidence);
    public sealed record CheckerRunResponse(string Verdict, string Explanation);

    [HttpPost("run")]
    public async Task<ActionResult<CheckerRunResponse>> Run([FromBody] CheckerRunRequest request, CancellationToken ct)
    {
        var reply = await client.RunCycleAsync(
            new Request { Intent = request.Intent, Evidence = request.Evidence },
            cancellationToken: ct);
        return Ok(new CheckerRunResponse(reply.Verdict, reply.Explanation));
    }
}
