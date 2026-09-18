using System.Windows.Input;

namespace ProjectName.App.Pages.Controls;

/// <summary>
/// Reusable marketplace category selector. The host supplies Category and Command.
/// </summary>
public partial class MarketplaceCategoryChip : ContentView
{
    public static readonly BindableProperty CategoryProperty = BindableProperty.Create(
        nameof(Category), typeof(string), typeof(MarketplaceCategoryChip), string.Empty);

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(MarketplaceCategoryChip));

    public string Category
    {
        get => (string)GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public MarketplaceCategoryChip() => InitializeComponent();
}