using System.Text.Json;
using ProjectName.App.Models;

namespace ProjectName.App.Services;

/// <summary>
/// Loads the generated DSO graph and resolves composition by id.
///
/// The single dictionary is the point: every parent that composes a reference gets
/// the same <see cref="Dso"/> instance back, so reuse is a property of the graph
/// rather than something the UI has to remember to fake.
/// </summary>
public sealed class DsoCatalog
{
    // Concrete array, not IReadOnlyList: a collection expression targeting a
    // non-mutable interface trips CsWinRT1032 under trimming/AOT.
    private readonly IDsoSource[] _sources;

    private DsoCatalogFile? _file;
    private Dictionary<string, Dso> _byId = [];

    public DsoCatalog(IEnumerable<IDsoSource> sources)
        => _sources = [.. sources.OrderBy(s => s.Priority)];

    public string Generated => _file?.Generated ?? "";

    /// <summary>Which source answered - shown on the catalog screen so staleness is visible.</summary>
    public string SourceName { get; private set; } = "";

    public async Task<DsoCatalogFile> LoadAsync(CancellationToken ct = default)
    {
        if (_file is not null)
        {
            return _file;
        }

        // First source that answers wins. Live outranks the bundled snapshot; the
        // snapshot exists so this still resolves with no network at all.
        foreach (var source in _sources)
        {
            var loaded = await source.TryLoadAsync(ct);
            if (loaded is not null)
            {
                _file = loaded;
                SourceName = source.Name;
                break;
            }
        }

        _file ??= new DsoCatalogFile();

        _byId = _file.Dsos
            .Where(d => !string.IsNullOrWhiteSpace(d.Id))
            .GroupBy(d => d.Id)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        return _file;
    }

    public async Task<Dso?> GetAsync(string id, CancellationToken ct = default)
    {
        await LoadAsync(ct);
        return _byId.TryGetValue(id ?? "", out var dso) ? dso : null;
    }

    public async Task<Dso[]> RootsAsync(CancellationToken ct = default)
    {
        var file = await LoadAsync(ct);
        return [.. file.Roots.Select(id => _byId.GetValueOrDefault(id)).OfType<Dso>()];
    }

    /// <summary>Composed resources, grouped by kind and ordered as the graph lists them.</summary>
    public async Task<DsoGroup[]> ResourcesAsync(Dso dso, CancellationToken ct = default)
    {
        await LoadAsync(ct);

        return
        [
            .. dso.Resources
                .Select(id => _byId.GetValueOrDefault(id))
                .OfType<Dso>()
                .GroupBy(d => d.Kind)
                .OrderBy(g => KindOrder(g.Key))
                .Select(g => new DsoGroup { Kind = g.Key, Items = [.. g] })
        ];
    }

    /// <summary>
    /// Every parent that composes this DSO. A reference reused by three skills should
    /// say so on its own screen - that is the reuse being visible, not just claimed.
    /// </summary>
    public async Task<Dso[]> UsedByAsync(Dso dso, CancellationToken ct = default)
    {
        var file = await LoadAsync(ct);
        return [.. file.Dsos.Where(d => d.Resources.Contains(dso.Id, StringComparer.OrdinalIgnoreCase))];
    }

    private static int KindOrder(string kind) => kind switch
    {
        "Skill" => 0,
        "Agent" => 1,
        "Reference" => 2,
        "Script" => 3,
        "Tool" => 4,
        "Asset" => 5,
        "Command" => 6,
        "Template" => 7,
        _ => 99,
    };
}
