using Microsoft.AspNetCore.Mvc;
using ProjectName.ReflectorGrpc;

namespace ProjectName.OrchestratorApi.Controllers;

/// <summary>
/// Thin HTTP facade over the stateless <see cref="Reflector.ReflectorClient"/> gRPC
/// service. This is a single MAF call, not a persisted trail — see
/// ProjectName.ReflectorGrpc.Services.ReflectorGrpcService for what it actually does.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ReflectorController(Reflector.ReflectorClient client) : ControllerBase
{
    public sealed record ReflectorRunRequest(string Intent, string CheckerVerdict, string CheckerEvidence);
    public sealed record ReflectorRunResponse(string Disposition, string Reflection);

    [HttpPost("run")]
    public async Task<ActionResult<ReflectorRunResponse>> Run([FromBody] ReflectorRunRequest request, CancellationToken ct)
    {
        var reply = await client.RunCycleAsync(
            new Request
            {
                Intent = request.Intent,
                CheckerVerdict = request.CheckerVerdict,
                CheckerEvidence = request.CheckerEvidence
            },
            cancellationToken: ct);
        return Ok(new ReflectorRunResponse(reply.Disposition, reply.Reflection));
    }
}
