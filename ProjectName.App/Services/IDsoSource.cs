using ProjectName.App.Models;

namespace ProjectName.App.Services;

/// <summary>
/// Where the DSO graph comes from.
///
/// The graph is not a build artifact by nature - it is whatever objects exist in the
/// skills tree right now. A bundled catalog is a cache of that, and it goes stale the
/// moment the tree changes. So the app asks sources in priority order and takes the
/// first that answers, rather than assuming the copy it shipped with is current.
/// </summary>
public interface IDsoSource
{
    /// <summary>Lower runs first. A live source should outrank the bundled cache.</summary>
    int Priority { get; }

    string Name { get; }

    /// <summary>Null means "I cannot answer right now" - the next source is tried.</summary>
    Task<DsoCatalogFile?> TryLoadAsync(CancellationToken ct = default);
}
