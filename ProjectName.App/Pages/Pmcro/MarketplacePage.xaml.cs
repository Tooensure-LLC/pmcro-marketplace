using ProjectName.App.PageModels;

namespace ProjectName.App.Pages.Pmcro;

public partial class MarketplacePage : ContentPage
{
    private readonly MarketplacePageModel _model;

    public MarketplacePage(MarketplacePageModel model)
    {
        InitializeComponent();
        BindingContext = _model = model;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _model.AppearingCommand.Execute(null);
    }
}
