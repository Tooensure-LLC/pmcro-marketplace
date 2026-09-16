using ProjectName.App.Models;
using ProjectName.App.PageModels;

namespace ProjectName.App.Pages;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}