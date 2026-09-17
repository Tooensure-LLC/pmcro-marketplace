using System.Text.Json;
using ProjectName.App.Models;

namespace ProjectName.App.Services;

/// <summary>
/// The catalog generated at build time by .tools/generate-dso-catalog.ps1 and shipped
/// as a MauiAsset.
///
/// This is the offline floor, not the source of truth: it is a snapshot of the skills
/// tree as it stood when the app was built. It runs last so a live source wins, and it
/// exists so the app still opens on a phone with no network.
/// </summary>
public sealed class BundledDsoSource : IDsoSource
{
    private const string AssetName = "DsoCatalog.json";

    public int Priority => 100;

    public string Name => "bundled";

    public async Task<DsoCatalogFile?> TryLoadAsync(CancellationToken ct = default)
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(AssetName);
            return await JsonSerializer.DeserializeAsync(stream, JsonContext.Default.DsoCatalogFile, ct);
        }
        catch (FileNotFoundException)
        {
            // Generator never run before packaging. Not fatal - the caller shows an
            // empty catalog with instructions rather than failing to open the screen.
            return null;
        }
    }
}
