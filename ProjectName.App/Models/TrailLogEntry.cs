using ProjectName.App.Utilities;

namespace ProjectName.App.Models;

/// <summary>
/// One line of the trail log, rendered monospaced: role, verb, then evidence.
/// Built from a <see cref="PmcrFrame"/> so the log is never hand-authored copy.
/// </summary>
public sealed class TrailLogEntry
{
    public required string Role { get; init; }

    /// <summary>frame | plan | make | check | reflect</summary>
    public required string Verb { get; init; }

    public required string Evidence { get; init; }

    public Color RoleColor => PmcroPalette.ForRole(Role);

    public static TrailLogEntry FromFrame(PmcrFrame frame) => new()
    {
        Role = frame.Role,
        Verb = VerbFor(frame.Role),
        Evidence = string.IsNullOrWhiteSpace(frame.Evidence) ? frame.Verdict : frame.Evidence,
    };

    private static string VerbFor(string role) => role?.Trim().ToLowerInvariant() switch
    {
        "orchestrator" => "frame",
        "planner" => "plan",
        "maker" => "make",
        "checker" => "check",
        "reflector" => "reflect",
        _ => "log",
    };
}
