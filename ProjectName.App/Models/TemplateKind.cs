using CommunityToolkit.Mvvm.ComponentModel;
using ProjectName.App.Utilities;

namespace ProjectName.App.Models;

/// <summary>
/// One of the four things a PMCR-O template can produce, per templates/template.schema.md:
/// skill, plugin, agent, product.
/// </summary>
public partial class TemplateKind : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CardBackground))]
    [NotifyPropertyChangedFor(nameof(CardStroke))]
    [NotifyPropertyChangedFor(nameof(TileBackground))]
    [NotifyPropertyChangedFor(nameof(TileTextColor))]
    private bool _isSelected;

    /// <summary>skill | plugin | agent | product - the template.schema.md value.</summary>
    public required string Key { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    /// <summary>Two-letter tile label: Sk, Pl, Ag, Pr.</summary>
    public required string Abbreviation { get; init; }

    public Color CardBackground => IsSelected
        ? PmcroPalette.Get("PmcroPrimarySubtle")
        : PmcroPalette.SurfaceRaised;

    public Color CardStroke => IsSelected ? PmcroPalette.Primary : PmcroPalette.Border;

    public Color TileBackground => IsSelected
        ? PmcroPalette.Primary
        : PmcroPalette.Get("PmcroSurfaceMuted");

    public Color TileTextColor => IsSelected
        ? PmcroPalette.Get("PmcroOnPrimary")
        : PmcroPalette.TextSecondary;
}
