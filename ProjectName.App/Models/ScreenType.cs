namespace ProjectName.App.Models;

/// <summary>
/// The closed set of screen shapes a skill can render as.
///
/// This is deliberately small and deliberately closed. A skill does not get its own
/// bespoke screen - it declares which of these shapes it is, and the app renders it.
/// A .NET MAUI skill and a splash skill are not two screens; they are two skills
/// rendering through shapes that already exist.
///
/// The failure mode this guards against is type proliferation: if every skill that
/// does not quite fit gets a new type, there is no reuse left and this is just
/// bespoke screens with extra steps. Adding a type should be rare and argued for.
/// </summary>
public static class ScreenType
{
    /// <summary>Identity: mark, name, one line. The first thing you see.</summary>
    public const string Splash = "splash";

    /// <summary>A browsable index of the objects this one composes.</summary>
    public const string Catalog = "catalog";

    /// <summary>A document. Knowledge the agent progressively discloses.</summary>
    public const string Reference = "reference";

    /// <summary>A reasoning workspace: objective, trail, plan, evidence, artifacts.</summary>
    public const string Agent = "agent";

    /// <summary>Key/value configuration the person edits.</summary>
    public const string Settings = "settings";

    /// <summary>Search, filter, featured. The marketplace shape.</summary>
    public const string Discovery = "discovery";

    public static readonly string[] All =
        [Splash, Catalog, Reference, Agent, Settings, Discovery];

    public static bool IsKnown(string? type)
        => !string.IsNullOrWhiteSpace(type) && All.Contains(type.Trim().ToLowerInvariant());

    /// <summary>
    /// A skill that declares no screen type still has to render. Falling back by kind
    /// keeps the catalog usable without forcing every SKILL.md to be annotated first.
    /// </summary>
    public static string ForKind(string kind) => kind switch
    {
        "Plugin" => Catalog,
        "Skill" => Catalog,
        "Reference" => Reference,
        "Agent" => Agent,
        "Script" or "Tool" => Reference,
        "Asset" => Reference,
        _ => Catalog,
    };

    public static string Resolve(string? declared, string kind)
        => IsKnown(declared) ? declared!.Trim().ToLowerInvariant() : ForKind(kind);

    public static string Label(string type) => type switch
    {
        Splash => "Splash",
        Catalog => "Catalog",
        Reference => "Reference",
        Agent => "Agent",
        Settings => "Settings",
        Discovery => "Discovery",
        _ => type,
    };
}
