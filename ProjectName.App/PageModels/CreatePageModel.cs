using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectName.App.Models;

namespace ProjectName.App.PageModels;

/// <summary>
/// The generator surface. It only ever *declares* what will exist - the paths below
/// mirror templates/template.schema.md's declares.files. Writing happens in a trail.
/// </summary>
public partial class CreatePageModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<TemplateKind> _kinds = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GenerateLabel))]
    private TemplateKind? _selectedKind;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    private string _pluginName = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    private string _artifactName = "";

    [ObservableProperty]
    private string _description = "";

    [ObservableProperty]
    private ObservableCollection<string> _declares = [];

    public string GenerateLabel => $"Generate {SelectedKind?.Title ?? "Skill"}";

    public bool CanGenerate =>
        !string.IsNullOrWhiteSpace(PluginName) && !string.IsNullOrWhiteSpace(ArtifactName);

    public CreatePageModel()
    {
        Kinds =
        [
            new TemplateKind { Key = "skill", Title = "Skill", Abbreviation = "Sk", Summary = "A SKILL.md + references." },
            new TemplateKind { Key = "plugin", Title = "Plugin", Abbreviation = "Pl", Summary = "Bundles skills into plugin.json." },
            new TemplateKind { Key = "agent", Title = "Agent", Abbreviation = "Ag", Summary = "A role definition (*.agent.md)." },
            new TemplateKind { Key = "product", Title = "Product", Abbreviation = "Pr", Summary = "A full installable product." },
        ];

        Select(Kinds[0]);
    }

    [RelayCommand]
    private void Select(TemplateKind kind)
    {
        foreach (var k in Kinds)
        {
            k.IsSelected = ReferenceEquals(k, kind);
        }

        SelectedKind = kind;
        RebuildDeclares();
    }

    partial void OnPluginNameChanged(string value) => RebuildDeclares();

    partial void OnArtifactNameChanged(string value) => RebuildDeclares();

    /// <summary>
    /// The complete list of paths the template can produce. The Checker verifies this
    /// list and nothing else, so it is shown before generation rather than after.
    /// </summary>
    private void RebuildDeclares()
    {
        var plugin = Placeholder(PluginName, "pmcro");
        var artifact = Placeholder(ArtifactName, "skill-name");

        Declares = new ObservableCollection<string>(SelectedKind?.Key switch
        {
            "plugin" =>
            [
                $"plugins/{plugin}/plugin.json",
                $"plugins/{plugin}/skills/",
                $"plugins/{plugin}/references/",
            ],
            "agent" =>
            [
                $"plugins/{plugin}/agents/{artifact}.agent.md",
                $"plugins/{plugin}/agents/{artifact}/references/laws.md",
            ],
            "product" =>
            [
                $"plugins/{plugin}/plugin.json",
                $"plugins/{plugin}/.claude-plugin/marketplace.json",
                $"plugins/{plugin}/skills/{artifact}/SKILL.md",
                $"plugins/{plugin}/agents/",
            ],
            _ =>
            [
                $"plugins/{plugin}/skills/{artifact}/SKILL.md",
                $"plugins/{plugin}/skills/{artifact}/references/laws.md",
            ],
        });
    }

    [RelayCommand]
    private static Task Back()
        => Shell.Current.GoToAsync("..");

    private static string Placeholder(string value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
