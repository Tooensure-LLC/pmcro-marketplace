using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProjectName.App.PageModels;

/// <summary>
/// The Settings tab. It also re-homes the two controls the flyout used to own -
/// theme selection and the orchestrator endpoint - so disabling the flyout for the
/// tab bar did not quietly remove them from the app.
/// </summary>
public partial class SettingsPageModel : ObservableObject
{
    public const string ApiUrlKey = "pmcr_api_url";
    public const string DefaultApiUrl = "http://localhost:5100/";

    [ObservableProperty]
    private string _apiUrl = "";

    [ObservableProperty]
    private string _apiUrlStatus = "";

    [ObservableProperty]
    private int _themeIndex;

    public string VersionLabel => $"v{AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";

    public SettingsPageModel()
    {
        ApiUrl = Preferences.Default.Get(ApiUrlKey, DefaultApiUrl);
        ThemeIndex = Application.Current?.UserAppTheme switch
        {
            AppTheme.Light => 1,
            AppTheme.Dark => 2,
            _ => 0,
        };
    }

    partial void OnThemeIndexChanged(int value)
    {
        var theme = value switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Unspecified,
        };

        if (Application.Current is not null)
        {
            Application.Current.UserAppTheme = theme;
        }

        // Persisted under the same key App reads on launch, so the choice survives restart.
        Preferences.Default.Set(App.ThemePreferenceKey, theme.ToString());
    }

    [RelayCommand]
    private void SaveApiUrl()
    {
        var candidate = ApiUrl?.Trim() ?? "";

        // A malformed endpoint would throw inside the HttpClient factory at resolve
        // time, which surfaces as an unrelated navigation crash - reject it here.
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            ApiUrlStatus = "Enter an absolute http(s) URL.";
            return;
        }

        Preferences.Default.Set(ApiUrlKey, candidate);
        ApiUrlStatus = "Saved - restart the app for the runner to pick it up.";
    }

    [RelayCommand]
    private void ResetApiUrl()
    {
        ApiUrl = DefaultApiUrl;
        Preferences.Default.Set(ApiUrlKey, DefaultApiUrl);
        ApiUrlStatus = "Reset to the local default.";
    }

    [RelayCommand]
    private static Task OpenMeta()
        => Shell.Current.GoToAsync("meta");

    [RelayCommand]
    private static Task OpenDashboard()
        => Shell.Current.GoToAsync("dashboard");

    [RelayCommand]
    private static Task OpenRunner()
        => Shell.Current.GoToAsync("runner");
}
