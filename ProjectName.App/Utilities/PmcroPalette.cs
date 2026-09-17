namespace ProjectName.App.Utilities;

/// <summary>
/// Resolves PMCR-O design tokens from the merged application resource dictionaries.
/// Page models express *state*; this is the only place state becomes a color, so
/// PmcroTokens.xaml stays the single source of truth even for code-driven UI.
/// </summary>
public static class PmcroPalette
{
    public static Color Get(string key)
    {
        var resources = Application.Current?.Resources;
        if (resources is not null && resources.TryGetValue(key, out var value) && value is Color color)
        {
            return color;
        }

        // Token missing means the dictionary was not merged - fail loud in Debug,
        // fall back to transparent in Release rather than inventing a color.
        System.Diagnostics.Debug.Fail($"PMCR-O token '{key}' not found in application resources.");
        return Colors.Transparent;
    }

    public static Color Bg => Get("PmcroBg");
    public static Color Surface => Get("PmcroSurface");
    public static Color SurfaceRaised => Get("PmcroSurfaceRaised");
    public static Color Border => Get("PmcroBorder");
    public static Color BorderStrong => Get("PmcroBorderStrong");

    public static Color TextHeading => Get("PmcroTextHeading");
    public static Color TextPrimary => Get("PmcroTextPrimary");
    public static Color TextSecondary => Get("PmcroTextSecondary");
    public static Color TextMuted => Get("PmcroTextMuted");

    public static Color Primary => Get("PmcroPrimary");
    public static Color Accent => Get("PmcroAccent");

    public static Color PhaseDone => Get("PmcroPhaseDone");
    public static Color PhaseActive => Get("PmcroPhaseActive");
    public static Color PhasePending => Get("PmcroPhasePending");
    public static Color PhaseHalted => Get("PmcroPhaseHalted");

    /// <summary>Agent role -> log color. Unknown roles read as secondary text, never as an agent.</summary>
    public static Color ForRole(string role) => role?.Trim().ToLowerInvariant() switch
    {
        "orchestrator" => Get("PmcroRoleOrchestrator"),
        "planner" => Get("PmcroRolePlanner"),
        "maker" => Get("PmcroRoleMaker"),
        "checker" => Get("PmcroRoleChecker"),
        "reflector" => Get("PmcroRoleReflector"),
        _ => Get("PmcroTextSecondary"),
    };
}
