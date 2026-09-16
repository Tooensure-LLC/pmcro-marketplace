using CommunityToolkit.Mvvm.Input;
using ProjectName.App.Models;

namespace ProjectName.App.PageModels;

public interface IProjectTaskPageModel
{
	IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
	bool IsBusy { get; }
}