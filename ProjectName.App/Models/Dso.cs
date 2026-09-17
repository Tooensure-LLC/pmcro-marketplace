using System.Text.Json.Serialization;
using ProjectName.App.Utilities;

namespace ProjectName.App.Models;

/// <summary>
/// A Domain-Specific Object: a bounded, reusable unit of the PMCR-O system.
///
/// Every PMCR-O resource is the same shape - a plugin, a skill, a reference document,
/// an asset, an agent definition, a hook. That is what lets one screen render all of
/// them, and what lets a reference be composed by two skills without being copied.
/// </summary>
public sealed class Dso
{
    /// <summary>Stable slug of the path. Composition and routing both key on this.</summary>
    public string Id { get; set; } = "";

    /// <summary>Plugin | Skill | Reference | Script | Asset | Agent | Tool | Command | Template</summary>
    public string Kind { get; set; } = "";

    public string Name { get; set; } = "";
    public string Summary { get; set; } = "";

    /// <summary>Path in the skills repo. The DSO's identity in the filesystem.</summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// The landing document - SKILL.md for a skill. It is the doorway, not the
    /// implementation: it says what this is and what it can disclose, and the rest
    /// stays behind <see cref="Resources"/> until something actually needs it.
    /// </summary>
    public string Landing { get; set; } = "";

    /// <summary>
    /// Ids of composed DSOs, never nested copies. A reference shared by two skills
    /// appears once in the catalog and twice here - reuse, not duplication.
    /// </summary>
    public List<string> Resources { get; set; } = [];

    /// <summary>
    /// The screen shape this object declares (SKILL.md front-matter `screen:`).
    /// Empty means it never declared one and falls back by kind - see <see cref="ScreenType"/>.
    /// </summary>
    public string Screen { get; set; } = "";

    [JsonIgnore]
    public string ResolvedScreen => ScreenType.Resolve(Screen, Kind);

    [JsonIgnore]
    public bool DeclaresScreen => ScreenType.IsKnown(Screen);

    [JsonIgnore]
    public bool HasLanding => !string.IsNullOrWhiteSpace(Landing);

    [JsonIgnore]
    public string Glyph => Kind switch
    {
        "Plugin" => "Pl",
        "Skill" => "Sk",
        "Reference" => "Rf",
        "Script" => "Sc",
        "Asset" => "As",
        "Agent" => "Ag",
        "Tool" => "Tl",
        "Command" => "Cm",
        "Template" => "Tp",
        _ => "??",
    };

    /// <summary>Tint per kind, resolved from the token dictionary - no literals here.</summary>
    [JsonIgnore]
    public Color Accent => Kind switch
    {
        "Plugin" => PmcroPalette.Get("PmcroRoleOrchestrator"),
        "Skill" => PmcroPalette.Primary,
        "Reference" => PmcroPalette.Get("PmcroRolePlanner"),
        "Script" => PmcroPalette.Get("PmcroRoleMaker"),
        "Asset" => PmcroPalette.Get("PmcroRoleReflector"),
        "Agent" => PmcroPalette.Accent,
        "Tool" => PmcroPalette.Get("PmcroRoleChecker"),
        _ => PmcroPalette.TextSecondary,
    };

    [JsonIgnore]
    public string SubtitleOrPath => string.IsNullOrWhiteSpace(Summary) ? Path : Summary;
}

public sealed class DsoCatalogFile
{
    public string Generated { get; set; } = "";
    public string Source { get; set; } = "";
    public List<string> Roots { get; set; } = [];
    public List<Dso> Dsos { get; set; } = [];
}

/// <summary>One kind of resource a DSO composes, grouped for display.</summary>
public sealed class DsoGroup
{
    public required string Kind { get; init; }
    public required List<Dso> Items { get; init; }
    public string Heading => $"{Kind.ToUpperInvariant()} ({Items.Count})";
}
