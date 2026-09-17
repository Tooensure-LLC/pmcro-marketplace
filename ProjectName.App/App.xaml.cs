using Microsoft.Extensions.DependencyInjection;

namespace ProjectName.App;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// The PMCR-O screens are authored dark and the tab bar is a dark surface, so a
		// light OS theme would put light-mode project pages behind dark chrome. Default
		// to dark; Settings writes this preference and wins on every later launch.
		var stored = Preferences.Default.Get(ThemePreferenceKey, nameof(AppTheme.Dark));
		UserAppTheme = Enum.TryParse<AppTheme>(stored, out var theme) ? theme : AppTheme.Dark;
	}

	public const string ThemePreferenceKey = "app_theme";

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// No-op unless PMCRO_SMOKE=1. See StartupSmokeCheck.
		StartupSmokeCheck.RunIfRequested();

		return new Window(new AppShell());
	}
}