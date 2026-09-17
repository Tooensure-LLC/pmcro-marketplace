using ProjectName.App.Models;

namespace ProjectName.App.Pages.Pmcro;

/// <summary>
/// Picks the layout for a DSO from its declared screen type.
///
/// This is the seam that keeps the app generic without making every screen a list of
/// links: one page, a closed set of shapes, and the object decides which shape it is.
/// A skill that wants to look different declares a different type - it does not get a
/// new page.
/// </summary>
public sealed class ScreenTypeSelector : DataTemplateSelector
{
    public DataTemplate? Splash { get; set; }
    public DataTemplate? Catalog { get; set; }
    public DataTemplate? Reference { get; set; }
    public DataTemplate? Agent { get; set; }
    public DataTemplate? Settings { get; set; }
    public DataTemplate? Discovery { get; set; }

    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        var type = item switch
        {
            Dso dso => dso.ResolvedScreen,
            string s => s,
            _ => ScreenType.Catalog,
        };

        return type switch
        {
            ScreenType.Splash => Splash ?? Catalog,
            ScreenType.Reference => Reference ?? Catalog,
            ScreenType.Agent => Agent ?? Catalog,
            ScreenType.Settings => Settings ?? Catalog,
            ScreenType.Discovery => Discovery ?? Catalog,
            _ => Catalog,
        };
    }
}
