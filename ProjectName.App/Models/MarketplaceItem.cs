using System.Text.Json.Serialization;

namespace ProjectName.App.Models;

/// <summary>One entry in the PMCR-O marketplace: a plugin, skill, theme or agent.</summary>
public sealed class MarketplaceItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Kind { get; set; } = "Plugin";

    /// <summary>Single letter shown in the icon tile - no image asset per plugin.</summary>
    public string Glyph { get; set; } = "?";

    /// <summary>Per-item brand tint. Everything else comes from PmcroTokens.xaml.</summary>
    public string Accent { get; set; } = "#7C6CF6";

    public string? Version { get; set; }
    public string? Source { get; set; }
    public bool Installed { get; set; }

    [JsonIgnore]
    public Color AccentColor => Color.FromArgb(Accent);

    [JsonIgnore]
    public string ActionText => Installed ? "Active" : "Get";

    [JsonIgnore]
    public string VersionLabel => string.IsNullOrWhiteSpace(Version) ? Kind : $"v{Version}";
}

public sealed class MarketplaceCatalogFile
{
    public string Name { get; set; } = "";
    public string Owner { get; set; } = "";
    public List<MarketplaceItem> Featured { get; set; } = [];
    public List<MarketplaceItem> Installed { get; set; } = [];
}
