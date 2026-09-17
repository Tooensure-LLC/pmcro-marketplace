using ProjectName.App.PageModels;

namespace ProjectName.App.Pages.Pmcro;

public partial class ActiveTrailPage : ContentPage
{
    private readonly ActiveTrailPageModel _model;

    public ActiveTrailPage(ActiveTrailPageModel model)
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
