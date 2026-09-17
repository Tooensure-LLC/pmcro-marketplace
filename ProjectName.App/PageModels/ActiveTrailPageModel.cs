using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectName.App.Models;
using ProjectName.App.Services;

namespace ProjectName.App.PageModels;

public partial class ActiveTrailPageModel : ObservableObject
{
    private static readonly string[] PhaseOrder =
        ["Frame", "Plan", "Make", "Check", "Reflect"];

    private readonly PmcrClient _client;
    private readonly TrailSession _session;
    private readonly ModalErrorHandler _errorHandler;

    [ObservableProperty]
    private ObservableCollection<TrailPhase> _phases = [];

    [ObservableProperty]
    private ObservableCollection<TrailLogEntry> _log = [];

    [ObservableProperty]
    private string _trailId = "no trail";

    [ObservableProperty]
    private string _trailLabel = "nothing running";

    [ObservableProperty]
    private bool _isBusy;

    // ---- Runtime badge: reflects GET /health, not a hardcoded "local" ----
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RuntimeLabel))]
    private bool _isOrchestratorUp;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RuntimeLabel))]
    private string _runtimeModel = "";

    public string RuntimeLabel => IsOrchestratorUp
        ? (string.IsNullOrWhiteSpace(RuntimeModel) ? "local" : RuntimeModel)
        : "offline";

    // ---- Action bar ----
    [ObservableProperty]
    private string _advanceLabel = "Open runner";

    [ObservableProperty]
    private bool _canAdvance = true;

    [ObservableProperty]
    private bool _canHalt;

    [ObservableProperty]
    private string _statusNote = "";

    public ActiveTrailPageModel(PmcrClient client, TrailSession session, ModalErrorHandler errorHandler)
    {
        _client = client;
        _session = session;
        _errorHandler = errorHandler;

        Phases = new ObservableCollection<TrailPhase>(BuildEmptyTrail());

        _session.CycleChanged += (_, cycle) => Apply(cycle);
        _session.FollowEnded += (_, reason) =>
        {
            CanHalt = false;
            StatusNote = reason switch
            {
                "halted" => "Stopped following. The orchestrator keeps running.",
                "settled" => "No new frames - the run has stopped producing evidence.",
                "superseded" => "",
                _ => $"Follow ended: {reason}",
            };
            UpdateAction();
        };
    }

    private static IEnumerable<TrailPhase> BuildEmptyTrail() =>
        PhaseOrder.Select((name, i) => new TrailPhase
        {
            Name = name,
            Artifact = $"{i:00}-{name.ToLowerInvariant()}.jsonl",
            HasNext = i < PhaseOrder.Length - 1,
            State = TrailPhaseState.Pending,
        });

    [RelayCommand]
    private async Task Appearing()
    {
        await ProbeRuntime();

        if (_session.Current is { } cycle)
        {
            Apply(cycle);
        }

        CanHalt = _session.IsFollowing;
        UpdateAction();
    }

    private async Task ProbeRuntime()
    {
        IsOrchestratorUp = await _client.IsHealthyAsync();
        RuntimeModel = IsOrchestratorUp
            ? (await _client.GetRuntimeAsync())?.Model ?? ""
            : "";
    }

    /// <summary>
    /// The bottom-right action. There is no approve endpoint, so this never claims to
    /// gate a phase - it follows, refreshes, or sends you to the runner.
    /// </summary>
    private void UpdateAction()
    {
        if (_session.IsFollowing)
        {
            AdvanceLabel = "Following…";
            CanAdvance = false;
        }
        else if (_session.Current is not null)
        {
            AdvanceLabel = "Refresh";
            CanAdvance = true;
        }
        else
        {
            AdvanceLabel = "Open runner";
            CanAdvance = true;
        }
    }

    [RelayCommand]
    private async Task Advance()
    {
        if (_session.Current is not { } cycle)
        {
            await Shell.Current.GoToAsync("runner");
            return;
        }

        try
        {
            IsBusy = true;
            // The route is /api/cycles/{trail.Id}; CycleId is the correlation id, not the key.
            var refreshed = await _client.GetCycleAsync(cycle.Id);
            if (refreshed is not null)
            {
                Apply(refreshed);
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
            await ProbeRuntime();
            UpdateAction();
        }
    }

    [RelayCommand]
    private void Halt()
    {
        _session.Halt();
        CanHalt = false;
        UpdateAction();
    }

    /// <summary>Project a cycle onto the five-node trail. No phase is done without a frame behind it.</summary>
    public void Apply(PmcrCycle cycle)
    {
        TrailId = Shorten(cycle.CycleId == Guid.Empty ? cycle.Id : cycle.CycleId);
        TrailLabel = string.IsNullOrWhiteSpace(cycle.Intent) ? cycle.SourceType : cycle.Intent;

        var reached = cycle.Frames
            .Select(f => IndexOfRole(f.Role))
            .Where(i => i >= 0)
            .DefaultIfEmpty(-1)
            .Max();

        var halted = cycle.Status?.Equals("halted", StringComparison.OrdinalIgnoreCase) == true;

        var phases = BuildEmptyTrail().ToList();
        for (var i = 0; i < phases.Count; i++)
        {
            phases[i].State = i < reached ? TrailPhaseState.Done
                : i == reached ? (halted ? TrailPhaseState.Halted : TrailPhaseState.Active)
                : TrailPhaseState.Pending;
        }

        Phases = new ObservableCollection<TrailPhase>(phases);
        Log = new ObservableCollection<TrailLogEntry>(cycle.Frames.Select(TrailLogEntry.FromFrame));

        CanHalt = _session.IsFollowing;
        UpdateAction();
    }

    private static int IndexOfRole(string role) => role?.Trim().ToLowerInvariant() switch
    {
        "orchestrator" => 0,
        "planner" => 1,
        "maker" => 2,
        "checker" => 3,
        "reflector" => 4,
        _ => -1,
    };

    private static string Shorten(Guid id)
    {
        var s = id.ToString("N");
        return s.Length <= 13 ? s : $"{s[..8]}…{s[^5..]}";
    }
}
