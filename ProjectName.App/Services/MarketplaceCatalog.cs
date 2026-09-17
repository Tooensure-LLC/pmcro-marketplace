using System.Text.Json;
using ProjectName.App.Models;

namespace ProjectName.App.Services;

/// <summary>
/// Reads the bundled marketplace catalog. The file ships as a MauiAsset so the
/// Home screen has real plugin names on first launch, before any network call.
/// </summary>
public sealed class MarketplaceCatalog
{
    private const string AssetName = "MarketplaceCatalog.json";

    private MarketplaceCatalogFile? _cache;

    public async Task<MarketplaceCatalogFile> LoadAsync(CancellationToken ct = default)
    {
        if (_cache is not null)
        {
            return _cache;
        }

        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(AssetName);
            var parsed = await JsonSerializer.DeserializeAsync(
                stream, JsonContext.Default.MarketplaceCatalogFile, ct);
            _cache = parsed ?? new MarketplaceCatalogFile();
        }
        catch (FileNotFoundException)
        {
            // Asset missing means a packaging problem, not a runtime one - show an
            // empty catalog rather than crashing the shell on launch.
            _cache = new MarketplaceCatalogFile();
        }

        return _cache;
    }

    /// <summary>All items, used by search and the category filter.</summary>
    public async Task<MarketplaceItem[]> AllAsync(CancellationToken ct = default)
    {
        var catalog = await LoadAsync(ct);
        return [.. catalog.Featured, .. catalog.Installed];
    }
}
