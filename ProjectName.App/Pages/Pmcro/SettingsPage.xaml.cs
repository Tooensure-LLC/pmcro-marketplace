using ProjectName.App.PageModels;

namespace ProjectName.App.Pages.Pmcro;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
