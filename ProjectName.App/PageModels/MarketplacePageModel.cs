using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectName.App.Models;
using ProjectName.App.Services;

namespace ProjectName.App.PageModels;

public partial class MarketplacePageModel : ObservableObject
{
    private readonly MarketplaceCatalog _catalog;
    private readonly TrailSession _session;
    private readonly ModalErrorHandler _errorHandler;

    public string[] Categories { get; } =
        ["All", "Plugins", "Skills", "Themes", "Agents"];

    [ObservableProperty]
    private ObservableCollection<MarketplaceItem> _featured = [];

    [ObservableProperty]
    private ObservableCollection<MarketplaceItem> _installed = [];

    [ObservableProperty]
    private string _selectedCategory = "All";

    [ObservableProperty]
    private string _searchText = "";

    // ---- Trail banner: the last sealed loop, surfaced on Home ----
    [ObservableProperty]
    private bool _hasTrailBanner;

    [ObservableProperty]
    private string _trailBannerStatus = "";

    [ObservableProperty]
    private string _trailBannerTitle = "";

    [ObservableProperty]
    private string _trailBannerDetail = "";

    [ObservableProperty]
    private string _trailBannerLoop = "";

    public MarketplacePageModel(MarketplaceCatalog catalog, TrailSession session, ModalErrorHandler errorHandler)
    {
        _catalog = catalog;
        _session = session;
        _errorHandler = errorHandler;

        // The banner follows the same run the Trails tab shows - one session, two views.
        _session.CycleChanged += (_, cycle) => ShowTrail(cycle);

        if (_session.Current is { } current)
        {
            ShowTrail(current);
        }
    }

    [RelayCommand]
    private async Task Appearing()
    {
        try
        {
            var catalog = await _catalog.LoadAsync();
            Featured = new ObservableCollection<MarketplaceItem>(catalog.Featured);
            Installed = new ObservableCollection<MarketplaceItem>(catalog.Installed);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
    }

    [RelayCommand]
    private void SelectCategory(string category)
        => SelectedCategory = category;

    /// <summary>Called by the trail runner when a loop seals, so Home reflects real state.</summary>
    public void ShowTrail(PmcrCycle cycle)
    {
        var status = cycle.Status ?? "";
        var sealedTrail = status.Equals("accept", StringComparison.OrdinalIgnoreCase)
            || status.Equals("sealed", StringComparison.OrdinalIgnoreCase);

        var label = string.IsNullOrWhiteSpace(cycle.SourceType) ? "trail" : cycle.SourceType;

        HasTrailBanner = true;
        TrailBannerStatus = sealedTrail ? "TRAIL SEALED"
            : string.IsNullOrWhiteSpace(status) ? "RUNNING"
            : status.ToUpperInvariant();
        TrailBannerTitle = string.IsNullOrWhiteSpace(status)
            ? label
            : $"{label} · {status.ToUpperInvariant()}";

        // The newest frame's evidence is what the loop last proved; fall back to the seed.
        var latest = cycle.Frames.LastOrDefault();
        TrailBannerDetail = string.IsNullOrWhiteSpace(latest?.Evidence)
            ? cycle.Intent
            : latest!.Evidence;

        TrailBannerLoop = $"loop {Math.Max(cycle.LoopCount, 1)}/3";
    }

    [RelayCommand]
    private static Task OpenTrail()
        => Shell.Current.GoToAsync("//trail");

    [RelayCommand]
    private static Task OpenCreate()
        => Shell.Current.GoToAsync("create");
}
