using ProjectName.App.Services;

namespace ProjectName.App.Pages;

/// <summary>
/// Seeds a cycle and hands it to <see cref="TrailSession"/>. It deliberately does not
/// render the frames itself - the Trails tab is the one viewer, so there is no second
/// copy of the trail state to drift out of sync.
/// </summary>
public partial class PmcrPage : ContentPage
{
    private readonly TrailSession _session;

    public PmcrPage(TrailSession session)
    {
        InitializeComponent();
        _session = session;
    }

    private async void RunCycle(object? sender, EventArgs e)
    {
        var intent = IntentEditor.Text?.Trim();
        if (string.IsNullOrWhiteSpace(intent))
        {
            StatusLabel.Text = "Enter a seed intent first.";
            return;
        }

        try
        {
            RunButton.IsEnabled = false;
            Busy.IsVisible = Busy.IsRunning = true;
            StatusLabel.Text = "Opening a trail…";

            var id = await _session.StartAsync(intent);
            if (id is null)
            {
                throw new InvalidOperationException("The orchestrator accepted the intent but returned no cycle id.");
            }

            StatusLabel.Text = "Trail opened. Following on the Trails tab.";
            await Shell.Current.GoToAsync("//trail");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Could not start a cycle: {ex.Message}";
        }
        finally
        {
            Busy.IsRunning = Busy.IsVisible = false;
            RunButton.IsEnabled = true;
        }
    }
}
