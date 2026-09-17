using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectName.App.Models;
using ProjectName.App.Services;

namespace ProjectName.App.PageModels;

/// <summary>
/// One page model for every DSO kind.
///
/// There is no PluginPage, SkillPage, ReferencePage. A DSO screen shows an identity,
/// a landing document, and the resources it composes - and each of those resources
/// opens the same screen. That recursion is the whole design: adding a new kind of
/// resource to the skills tree needs a catalog regeneration, not a new page.
/// </summary>
[QueryProperty(nameof(DsoId), "id")]
public partial class DsoScreenPageModel : ObservableObject
{
    private readonly DsoCatalog _catalog;
    private readonly ModalErrorHandler _errorHandler;

    [ObservableProperty]
    private string _dsoId = "";

    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private string _kind = "";

    [ObservableProperty]
    private string _summary = "";

    [ObservableProperty]
    private string _path = "";

    [ObservableProperty]
    private string _landing = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLanding))]
    private bool _landingExists;

    public bool HasLanding => LandingExists;

    /// <summary>
    /// The object being rendered, as a one-item collection. That is what lets a
    /// DataTemplateSelector choose the screen shape - the object picks its own layout
    /// instead of the page hard-coding one.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Dso> _surface = [];

    [ObservableProperty]
    private ObservableCollection<DsoGroup> _groups = [];

    /// <summary>Parents composing this DSO - reuse made visible from the child's side.</summary>
    [ObservableProperty]
    private ObservableCollection<Dso> _usedBy = [];

    /// <summary>Which shape rendered, and whether the object asked for it.</summary>
    [ObservableProperty]
    private string _screenLabel = "";

    [ObservableProperty]
    private bool _isRoot;

    [ObservableProperty]
    private string _emptyNote = "";

    /// <summary>Free-text filter used by the discovery shape.</summary>
    [ObservableProperty]
    private string _filter = "";

    public DsoScreenPageModel(DsoCatalog catalog, ModalErrorHandler errorHandler)
    {
        _catalog = catalog;
        _errorHandler = errorHandler;
    }

    partial void OnDsoIdChanged(string value) => _ = LoadAsync(value);

    [RelayCommand]
    private async Task Appearing()
    {
        if (Groups.Count == 0 && string.IsNullOrEmpty(Title))
        {
            await LoadAsync(DsoId);
        }
    }

    private async Task LoadAsync(string id)
    {
        try
        {
            // No id means the screen was opened as the catalog root: show the plugins.
            if (string.IsNullOrWhiteSpace(id))
            {
                var roots = await _catalog.RootsAsync();
                IsRoot = true;
                Title = "Objects";
                Kind = "Catalog";
                Summary = "Every PMCR-O resource, as a reusable object.";
                // Say where the graph came from - a stale bundled snapshot should look
                // different from a live one, not silently identical.
                Path = _catalog.Generated is { Length: > 0 } g
                    ? $"{_catalog.SourceName} · generated {g[..10]}"
                    : _catalog.SourceName;
                LandingExists = false;
                UsedBy = [];
                ScreenLabel = ScreenType.Label(ScreenType.Catalog);
                Surface = [new Dso { Id = "", Kind = "Plugin", Name = "Objects", Screen = ScreenType.Catalog }];
                Groups = [new DsoGroup { Kind = "Plugin", Items = [.. roots] }];
                EmptyNote = roots.Length == 0
                    ? "Catalog is empty - run .tools/generate-dso-catalog.ps1 and rebuild."
                    : "";
                return;
            }

            var dso = await _catalog.GetAsync(id);
            if (dso is null)
            {
                IsRoot = false;
                Title = "Not in the catalog";
                Kind = "";
                Summary = $"No object with id '{id}'.";
                Path = "";
                LandingExists = false;
                Groups = [];
                UsedBy = [];
                Surface = [];
                ScreenLabel = "";
                EmptyNote = "The catalog may be stale - regenerate it from the skills tree.";
                return;
            }

            IsRoot = false;
            Title = dso.Name;
            Kind = dso.Kind;
            Summary = dso.Summary;
            Path = dso.Path;
            Landing = dso.Landing;
            LandingExists = dso.HasLanding;

            // Declared screen wins; otherwise the kind decides. Either way the object
            // carries its own shape and the page does not branch on it.
            ScreenLabel = ScreenType.Label(dso.ResolvedScreen)
                + (dso.DeclaresScreen ? "" : " (by kind)");
            Surface = [dso];

            Groups = [.. await _catalog.ResourcesAsync(dso)];
            UsedBy = [.. await _catalog.UsedByAsync(dso)];

            EmptyNote = Groups.Count == 0
                ? $"This {dso.Kind.ToLowerInvariant()} composes nothing - it is a leaf object."
                : "";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
    }

    /// <summary>Open another DSO on this same screen. Reuse is navigation, not duplication.</summary>
    [RelayCommand]
    private static Task Open(Dso dso)
        => dso is null || string.IsNullOrWhiteSpace(dso.Id)
            ? Task.CompletedTask
            : Shell.Current.GoToAsync($"dso?id={dso.Id}");

    [RelayCommand]
    private static Task OpenTrail()
        => Shell.Current.GoToAsync("//trail");

    [RelayCommand]
    private static Task OpenRunner()
        => Shell.Current.GoToAsync("runner");
}
