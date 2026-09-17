using ProjectName.App.Models;

namespace ProjectName.App.Services;

/// <summary>
/// The single source of truth for "which cycle is the app currently watching".
///
/// The orchestrator exposes no approve or halt endpoint - POST /api/cycles starts a
/// run that proceeds on its own, and GET /api/cycles/{id} reports where it got to.
/// So this observes; it does not drive. Halt stops this client following the run, it
/// does not stop the orchestrator.
/// </summary>
public sealed class TrailSession
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    /// <summary>Stop polling a run that never settles rather than following it forever.</summary>
    private static readonly TimeSpan PollCeiling = TimeSpan.FromMinutes(10);

    private readonly PmcrClient _client;
    private CancellationTokenSource? _polling;

    public TrailSession(PmcrClient client) => _client = client;

    /// <summary>Raised on the UI thread whenever the watched cycle changes.</summary>
    public event EventHandler<PmcrCycle>? CycleChanged;

    /// <summary>Raised when following stops - completed, halted by the user, or failed.</summary>
    public event EventHandler<string>? FollowEnded;

    public PmcrCycle? Current { get; private set; }

    public bool IsFollowing => _polling is { IsCancellationRequested: false };

    /// <summary>Start a cycle from a seed intent and follow it.</summary>
    public async Task<Guid?> StartAsync(string intent, CancellationToken ct = default)
    {
        var id = await _client.StartCycleAsync(intent, ct);
        if (id is Guid cycleId)
        {
            Follow(cycleId);
        }

        return id;
    }

    /// <summary>Attach to an already-running cycle and poll until it settles.</summary>
    public void Follow(Guid cycleId)
    {
        Halt("superseded");

        var cts = new CancellationTokenSource();
        _polling = cts;
        _ = PollAsync(cycleId, cts.Token);
    }

    public void Halt(string reason = "halted")
    {
        var cts = _polling;
        _polling = null;

        if (cts is null)
        {
            return;
        }

        cts.Cancel();
        cts.Dispose();
        Raise(() => FollowEnded?.Invoke(this, reason));
    }

    private async Task PollAsync(Guid cycleId, CancellationToken ct)
    {
        var deadline = DateTimeOffset.UtcNow + PollCeiling;
        var lastFrameCount = -1;
        var settledPasses = 0;

        try
        {
            while (!ct.IsCancellationRequested && DateTimeOffset.UtcNow < deadline)
            {
                var cycle = await _client.GetCycleAsync(cycleId, ct);

                if (cycle is not null)
                {
                    Current = cycle;
                    Raise(() => CycleChanged?.Invoke(this, cycle));

                    if (IsTerminal(cycle.Status))
                    {
                        Raise(() => FollowEnded?.Invoke(this, cycle.Status));
                        return;
                    }

                    // No new frames across several passes means the run is done even if
                    // the status field never reached a terminal value.
                    settledPasses = cycle.Frames.Count == lastFrameCount ? settledPasses + 1 : 0;
                    lastFrameCount = cycle.Frames.Count;

                    if (settledPasses >= 5 && cycle.Frames.Count > 0)
                    {
                        Raise(() => FollowEnded?.Invoke(this, "settled"));
                        return;
                    }
                }

                await Task.Delay(PollInterval, ct);
            }

            if (!ct.IsCancellationRequested)
            {
                Raise(() => FollowEnded?.Invoke(this, "timed out"));
            }
        }
        catch (OperationCanceledException)
        {
            // Halt() - already reported.
        }
        catch (Exception ex)
        {
            Raise(() => FollowEnded?.Invoke(this, $"lost the orchestrator: {ex.Message}"));
        }
    }

    private static bool IsTerminal(string? status) => status?.ToLowerInvariant() switch
    {
        "accept" or "accepted" or "sealed" or "reject" or "halted" or "failed" => true,
        _ => false,
    };

    /// <summary>Polling runs off the UI thread; every subscriber is a page model.</summary>
    private static void Raise(Action action)
    {
        if (MainThread.IsMainThread)
        {
            action();
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(action);
        }
    }
}
