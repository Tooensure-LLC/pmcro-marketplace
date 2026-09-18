using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using ProjectName.App.Services;
using ProjectName.App.Resources.Fonts;
using ProjectName.MauiServiceDefaults;

namespace ProjectName.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureSyncfusionToolkit()
			.ConfigureMauiHandlers(handlers =>
			{
#if WINDOWS
				Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
				{
					handler.PlatformView.SingleSelectionFollowsFocus = false;
				});

				Microsoft.Maui.Handlers.ContentViewHandler.Mapper.AppendToMapping(nameof(Pages.Controls.CategoryChart), (handler, view) =>
				{
					if (view is Pages.Controls.CategoryChart && handler.PlatformView is Microsoft.Maui.Platform.ContentPanel contentPanel)
					{
						contentPanel.IsTabStop = true;
					}
				});
#endif
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
				fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);

				// PMCR-O type. Each weight is registered as its own family: MAUI resolves
				// a FontFamily to one face, so asking for Bold on a Regular registration
				// gets a synthesised (smeared) bold rather than the real cut.
				fonts.AddFont("Inter-Regular.ttf", "Inter");
				fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
				fonts.AddFont("Inter-Bold.ttf", "InterBold");
				fonts.AddFont("JetBrainsMono-Regular.ttf", "Mono");
				fonts.AddFont("JetBrainsMono-Bold.ttf", "MonoBold");
			});

#if DEBUG
		builder.Logging.AddDebug();
		builder.Services.AddLogging(configure => configure.AddDebug());
#endif

		// Aspire service discovery + OTLP tracing. When run under the AppHost
		// (Windows device / Android emulator heads), this picks up
		// OTEL_EXPORTER_OTLP_ENDPOINT and the service-discovery config Aspire
		// injects; running the app standalone (outside AppHost) is still fine -
		// AddOpenTelemetryExporters() no-ops when that env var is unset.
		builder.AddServiceDefaults();

		builder.Services.AddSingleton<ProjectRepository>();
		builder.Services.AddSingleton<TaskRepository>();
		builder.Services.AddSingleton<CategoryRepository>();
		builder.Services.AddSingleton<TagRepository>();
		builder.Services.AddSingleton<SeedDataService>();
		builder.Services.AddSingleton<ModalErrorHandler>();
		builder.Services.AddSingleton<MainPageModel>();
		builder.Services.AddSingleton<ProjectListPageModel>();
		builder.Services.AddSingleton<ManageMetaPageModel>();

		builder.Services.AddTransientWithShellRoute<ProjectDetailPage, ProjectDetailPageModel>("project");
		builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");

		// ---- PMCR-O surface ----
		// The orchestrator endpoint is per-machine, so it stays in Preferences rather
		// than appsettings; Settings can rewrite it without a rebuild.
		builder.Services.AddSingleton(_ => new PmcrClient(new HttpClient
		{
			BaseAddress = new Uri(Preferences.Default.Get("pmcr_api_url", "http://localhost:5100/")),
			Timeout = TimeSpan.FromMinutes(5),
		}));

		builder.Services.AddSingleton<MarketplaceCatalog>();

		// One session watched by both Home and Trails, so they never disagree.
		builder.Services.AddSingleton<TrailSession>();
		builder.Services.AddTransient<PmcrPage>();

		builder.Services.AddSingleton<ActiveTrailPageModel>();
		builder.Services.AddSingleton<MarketplacePageModel>();
		builder.Services.AddSingleton<Pages.Pmcro.ActiveTrailPage>();
		builder.Services.AddSingleton<Pages.Pmcro.MarketplacePage>();
		builder.Services.AddSingleton<SettingsPageModel>();
		builder.Services.AddSingleton<Pages.Pmcro.SettingsPage>();

		// One page for every DSO kind; the object's declared screen type picks the shape.
		// Sources are tried by priority: live orchestrator first, bundled snapshot last.
		builder.Services.AddSingleton<IDsoSource, RemoteDsoSource>();
		builder.Services.AddSingleton<IDsoSource, BundledDsoSource>();
		builder.Services.AddSingleton<DsoCatalog>();
		builder.Services.AddTransientWithShellRoute<Pages.Pmcro.DsoScreenPage, DsoScreenPageModel>("dso");

		// Create is pushed from the Home FAB rather than owning a tab.
		builder.Services.AddTransientWithShellRoute<Pages.Pmcro.CreatePage, CreatePageModel>("create");

		// Kept off the tab bar but still navigable, so the shell change orphans nothing.
		Routing.RegisterRoute("dashboard", typeof(MainPage));
		Routing.RegisterRoute("runner", typeof(PmcrPage));
		Routing.RegisterRoute("meta", typeof(ManageMetaPage));

		
		return builder.Build();
	}
}
