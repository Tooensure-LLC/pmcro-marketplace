using System.Net.Http.Json;
using ProjectName.App.Models;

namespace ProjectName.App.Services;

public sealed class PmcrClient
{
    private readonly HttpClient _http;
    public PmcrClient(HttpClient http) => _http = http;

    public async Task<PmcrCycle?> CreateCycleAsync(string intent, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/cycles", new { intent }, ct);
        response.EnsureSuccessStatusCode();
        var accepted = await response.Content.ReadFromJsonAsync<CycleAccepted>(cancellationToken: ct);
        return accepted is null ? null : await GetCycleAsync(accepted.Id, ct);
    }

    public Task<PmcrCycle?> GetCycleAsync(Guid id, CancellationToken ct = default)
        => _http.GetFromJsonAsync<PmcrCycle>($"api/cycles/{id}", ct);

    /// <summary>
    /// Starts a cycle and returns its id without waiting for the run to finish.
    /// POST /api/cycles answers 202 the moment the orchestrator accepts the intent;
    /// CreateCycleAsync's immediate GET only sees the first frame or two, so anything
    /// that wants to watch the loop should poll instead.
    /// </summary>
    public async Task<Guid?> StartCycleAsync(string intent, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/cycles", new { intent }, ct);
        response.EnsureSuccessStatusCode();
        var accepted = await response.Content.ReadFromJsonAsync<CycleAccepted>(cancellationToken: ct);
        return accepted?.Id;
    }

    /// <summary>GET /health - true when the orchestrator answers, false on any failure.</summary>
    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        try
        {
            using var response = await _http.GetAsync("health", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception) when (ct.IsCancellationRequested is false)
        {
            // An unreachable orchestrator is the normal offline state, not an error
            // worth surfacing as an alert - the caller renders it as a status dot.
            return false;
        }
    }

    public async Task<PmcrRuntime?> GetRuntimeAsync(CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<PmcrRuntime>("api/runtime", ct);
        }
        catch (Exception) when (ct.IsCancellationRequested is false)
        {
            return null;
        }
    }

    /// <summary>Generic GET for endpoints that may not exist yet; callers handle null.</summary>
    public Task<T?> GetJsonAsync<T>(string path, CancellationToken ct = default)
        => _http.GetFromJsonAsync<T>(path, ct);

    private sealed record CycleAccepted(Guid Id, string Status);
}
