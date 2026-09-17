using ProjectName.App.PageModels;

namespace ProjectName.App.Pages.Pmcro;

public partial class DsoScreenPage : ContentPage
{
    private readonly DsoScreenPageModel _model;

    public DsoScreenPage(DsoScreenPageModel model)
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
