using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ProjectName.App.Pages;

namespace ProjectName.App.Services;

/// <summary>
/// Resolves every page the shell can reach and reports which ones construct.
///
/// A build cannot catch a missing registration or a changed constructor - those fail
/// the first time a tab is opened, which is easy to miss when only one tab gets
/// exercised. This runs the same resolution the shell performs (container first, then
/// ActivatorUtilities, which is how ContentTemplate resolves unregistered page types).
///
/// Opt-in only: set PMCRO_SMOKE=1. It never runs in normal use.
/// </summary>
public static class StartupSmokeCheck
{
    public const string EnvVar = "PMCRO_SMOKE";

    private static readonly (string Route, Type Page)[] Pages =
    [
        ("home", typeof(Pages.Pmcro.MarketplacePage)),
        ("discover", typeof(ProjectListPage)),
        ("trail", typeof(Pages.Pmcro.ActiveTrailPage)),
        ("settings", typeof(Pages.Pmcro.SettingsPage)),
        ("create", typeof(Pages.Pmcro.CreatePage)),
        ("dso", typeof(Pages.Pmcro.DsoScreenPage)),
        ("runner", typeof(PmcrPage)),
        ("dashboard", typeof(MainPage)),
        ("meta", typeof(ManageMetaPage)),
        ("project", typeof(ProjectDetailPage)),
        ("task", typeof(TaskDetailPage)),
    ];

    public static void RunIfRequested()
    {
        if (Environment.GetEnvironmentVariable(EnvVar) != "1")
        {
            return;
        }

        var services = IPlatformApplication.Current?.Services;
        var report = new StringBuilder();
        var failures = 0;

        if (services is null)
        {
            report.AppendLine("FAIL  service provider unavailable");
            failures++;
        }
        else
        {
            foreach (var (route, page) in Pages)
            {
                try
                {
                    var instance = services.GetService(page)
                        ?? ActivatorUtilities.CreateInstance(services, page);

                    report.AppendLine($"ok    {route,-10} {page.Name} -> {instance.GetType().Name}");
                }
                catch (Exception ex)
                {
                    failures++;
                    report.AppendLine($"FAIL  {route,-10} {page.Name}: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        report.AppendLine($"---- {Pages.Length - failures}/{Pages.Length} resolved, {failures} failed");

        var path = Path.Combine(FileSystem.CacheDirectory, "pmcro-smoke.txt");
        File.WriteAllText(path, report.ToString());
    }
}
