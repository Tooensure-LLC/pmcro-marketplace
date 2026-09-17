// src/ProjectName.ServiceDefaults/MarketplaceSkillsMaterializer.cs
//
// ARCH-MAF-NATIVE-002: skills are not embedded into service binaries via
// Content Include. Instead, this materializer reads .agents/plugins/marketplace.json
// at repo root and mirrors each registered plugin's skills/<name>/ folders into
// .pmcro/skills-staging (relative to the host's content root / AppContext.BaseDirectory)
// before MafPhaseRunner constructs the native AgentSkillsProvider. See MafExtensions.cs.
//
// Previously referenced only in comments (MafExtensions.cs, the five *Grpc .csproj
// files) with no implementation - MafPhaseRunner.RunAsync's Directory.Exists(skillsPath)
// check silently returned Array.Empty<AIContextProvider>() for every run, so skills
// were never surfaced to any agent even though every controller returned genuine 200s.
using System.Text.Json;

namespace ProjectName.ServiceDefaults;

public static class MarketplaceSkillsMaterializer
{
    private const string MarketplaceRelativePath = ".agents/plugins/marketplace.json";

    /// <summary>
    /// Locates the repo root by walking up from <paramref name="searchStart"/> until
    /// .agents/plugins/marketplace.json is found, reads the marketplace's registered
    /// plugins, and copies each plugin's skills/&lt;skill&gt;/ folder (any folder directly
    /// containing a SKILL.md) into <paramref name="skillsStagingPath"/>/&lt;skill&gt;/.
    /// Safe to call repeatedly (each call re-mirrors, overwriting existing files).
    /// No-ops and returns 0 if the marketplace manifest cannot be located - this is
    /// intentionally non-fatal so a host still starts (without skills) when run
    /// somewhere the source tree isn't available (e.g. a container image that only
    /// has the published bin output).
    /// </summary>
    public static int Materialize(string skillsStagingPath, string? searchStart = null)
    {
        var repoRoot = FindRepoRoot(searchStart ?? AppContext.BaseDirectory);
        if (repoRoot is null)
            return 0;

        var marketplacePath = Path.Combine(repoRoot, ToNativePath(MarketplaceRelativePath));
        if (!File.Exists(marketplacePath))
            return 0;

        using var doc = JsonDocument.Parse(File.ReadAllText(marketplacePath));
        if (!doc.RootElement.TryGetProperty("plugins", out var pluginsEl) || pluginsEl.ValueKind != JsonValueKind.Array)
            return 0;

        Directory.CreateDirectory(skillsStagingPath);
        var copied = 0;

        foreach (var plugin in pluginsEl.EnumerateArray())
        {
            if (!plugin.TryGetProperty("source", out var sourceEl))
                continue;

            var source = sourceEl.GetString();
            if (string.IsNullOrWhiteSpace(source))
                continue;

            // "source" (e.g. "./plugins/pmcro") is relative to repo root, NOT to
            // marketplace.json's own directory (.agents/plugins/) - repo root here
            // has a top-level plugins/ folder, confirmed against the actual layout.
            var pluginSkillsDir = Path.Combine(Path.GetFullPath(Path.Combine(repoRoot, source)), "skills");
            if (!Directory.Exists(pluginSkillsDir))
                continue;

            foreach (var skillDir in Directory.GetDirectories(pluginSkillsDir))
            {
                if (!File.Exists(Path.Combine(skillDir, "SKILL.md")))
                    continue;

                var destDir = Path.Combine(skillsStagingPath, Path.GetFileName(skillDir));
                MirrorDirectory(skillDir, destDir);
                copied++;
            }
        }

        return copied;
    }

    private static void MirrorDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);
        }
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            MirrorDirectory(subDir, Path.Combine(destDir, Path.GetFileName(subDir)));
        }
    }

    private static string? FindRepoRoot(string start)
    {
        var marker = ToNativePath(MarketplaceRelativePath);
        var dir = new DirectoryInfo(start);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, marker)))
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }

    private static string ToNativePath(string relativeUnixPath) =>
        relativeUnixPath.Replace('/', Path.DirectorySeparatorChar);
}
