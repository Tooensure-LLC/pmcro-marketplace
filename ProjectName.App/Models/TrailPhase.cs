using CommunityToolkit.Mvvm.ComponentModel;
using ProjectName.App.Utilities;

namespace ProjectName.App.Models;

public enum TrailPhaseState
{
    Pending,
    Active,
    Done,
    Halted,
}

/// <summary>
/// One node on the PMCR-O trail: Frame, Plan, Make, Check, Reflect.
/// Each phase names the jsonl artifact it writes - Log Before Act means the
/// artifact exists before the phase is allowed to advance.
/// </summary>
public partial class TrailPhase : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NodeBackground))]
    [NotifyPropertyChangedFor(nameof(NodeStroke))]
    [NotifyPropertyChangedFor(nameof(ConnectorColor))]
    [NotifyPropertyChangedFor(nameof(NameColor))]
    [NotifyPropertyChangedFor(nameof(ArtifactColor))]
    [NotifyPropertyChangedFor(nameof(ShowCheck))]
    private TrailPhaseState _state = TrailPhaseState.Pending;

    public string Name { get; init; } = "";

    /// <summary>The jsonl the phase writes, e.g. "01-plan.jsonl".</summary>
    public string Artifact { get; init; } = "";

    /// <summary>False on the last phase so no connector hangs below Reflect.</summary>
    public bool HasNext { get; init; } = true;

    public bool IsComplete => State is TrailPhaseState.Done;

    public bool ShowCheck => State is TrailPhaseState.Done;

    public Color NodeBackground => State switch
    {
        TrailPhaseState.Done => PmcroPalette.PhaseDone,
        TrailPhaseState.Active => PmcroPalette.PhaseActive,
        TrailPhaseState.Halted => PmcroPalette.PhaseHalted,
        _ => Colors.Transparent,
    };

    public Color NodeStroke => State switch
    {
        TrailPhaseState.Done or TrailPhaseState.Active => PmcroPalette.PhaseDone,
        TrailPhaseState.Halted => PmcroPalette.PhaseHalted,
        _ => PmcroPalette.PhasePending,
    };

    /// <summary>The connector is lit only for ground the loop has actually covered.</summary>
    public Color ConnectorColor => State is TrailPhaseState.Done
        ? PmcroPalette.PhaseDone
        : PmcroPalette.PhasePending;

    public Color NameColor => State is TrailPhaseState.Pending
        ? PmcroPalette.TextMuted
        : PmcroPalette.TextHeading;

    public Color ArtifactColor => State is TrailPhaseState.Pending
        ? PmcroPalette.TextMuted
        : PmcroPalette.TextSecondary;
}
