namespace ProjectName.App.Models;

public sealed class PmcrCycle
{
    public Guid Id { get; set; }
    public Guid CycleId { get; set; }
    public string Intent { get; set; } = "";
    public string SourceType { get; set; } = "";
    public string Status { get; set; } = "";
    public int LoopCount { get; set; }
    public List<PmcrFrame> Frames { get; set; } = [];
}

/// <summary>Shape of GET /api/runtime - what the orchestrator says it is running.</summary>
public sealed class PmcrRuntime
{
    public string Service { get; set; } = "";
    public string Model { get; set; } = "";
    public List<string> Phases { get; set; } = [];
}

public sealed class PmcrFrame
{
    public Guid Id { get; set; }
    public string Role { get; set; } = "";
    public int CycleNumber { get; set; }
    public string Verdict { get; set; } = "";
    public string TypedEnvelopeJson { get; set; } = "";
    public string Evidence { get; set; } = "";
}
