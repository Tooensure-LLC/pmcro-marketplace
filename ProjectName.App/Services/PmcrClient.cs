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

    private sealed record CycleAccepted(Guid Id, string Status);
}
