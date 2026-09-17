using ProjectName.App.PageModels;

namespace ProjectName.App.Pages.Pmcro;

public partial class CreatePage : ContentPage
{
    public CreatePage(CreatePageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
