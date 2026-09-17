using System.Net.Http.Json;
using ProjectName.App.Models;

namespace ProjectName.App.Services;

/// <summary>
/// The live DSO graph, served by the orchestrator.
///
/// NOT YET AVAILABLE. The orchestrator currently exposes /health, /api/cycles,
/// /api/cycles/{id} and /api/runtime - there is no catalog endpoint, and no MCP
/// surface either. This class is the seam, kept honest: it asks, and when the
/// endpoint is absent it returns null so the bundled cache answers instead.
///
/// It is written against a plain HTTP contract deliberately. MCP is the likely
/// long-term transport, but MCP carries the same JSON this expects - so when the
/// server side lands, only <see cref="TryLoadAsync"/> changes, not the model, not
/// the catalog, and not a single screen.
/// </summary>
public sealed class RemoteDsoSource : IDsoSource
{
    /// <summary>The contract this expects, whenever something implements it.</summary>
    public const string Endpoint = "api/dso/catalog";

    private readonly PmcrClient _client;

    /// <summary>Set once a probe fails, so every screen does not re-await a timeout.</summary>
    private bool _knownUnavailable;

    public RemoteDsoSource(PmcrClient client) => _client = client;

    public int Priority => 10;

    public string Name => "orchestrator";

    public async Task<DsoCatalogFile?> TryLoadAsync(CancellationToken ct = default)
    {
        if (_knownUnavailable)
        {
            return null;
        }

        try
        {
            var catalog = await _client.GetJsonAsync<DsoCatalogFile>(Endpoint, ct);
            if (catalog is null || catalog.Dsos.Count == 0)
            {
                _knownUnavailable = true;
                return null;
            }

            return catalog;
        }
        catch (Exception) when (!ct.IsCancellationRequested)
        {
            // No catalog endpoint, or no orchestrator. Both are the normal state today.
            _knownUnavailable = true;
            return null;
        }
    }
}
