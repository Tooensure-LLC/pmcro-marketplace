using ProjectName.App.Services;

namespace ProjectName.App.Pages;

public partial class PmcrPage : ContentPage
{
    private readonly PmcrClient _client;
    public PmcrPage()
    {
        InitializeComponent();
        var baseUrl = Preferences.Default.Get("pmcr_api_url", "http://localhost:5100/");
        _client = new PmcrClient(new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromMinutes(5) });
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
            StatusLabel.Text = "Running Orchestrator → Planner → Maker → Checker → Reflector…";
            var cycle = await _client.CreateCycleAsync(intent);
            if (cycle is null) throw new InvalidOperationException("No cycle was returned.");
            StatusLabel.Text = $"Disposition: {cycle.Status.ToUpperInvariant()}";
            CycleLabel.Text = $"Cycle {cycle.CycleId} · Mode {cycle.SourceType}";
            Frames.ItemsSource = cycle.Frames;
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Runtime error: {ex.Message}";
        }
        finally
        {
            Busy.IsRunning = Busy.IsVisible = false;
            RunButton.IsEnabled = true;
        }
    }
}
