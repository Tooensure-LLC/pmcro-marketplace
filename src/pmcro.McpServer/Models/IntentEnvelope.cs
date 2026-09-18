using System.Text.Json.Serialization;

namespace Pmcro.McpServer.Models;

public sealed record IntentEnvelope
{
    [JsonPropertyName("cycle_id")] public required string CycleId { get; init; }
    [JsonPropertyName("seed_intent")] public required string SeedIntent { get; init; }
    [JsonPropertyName("truest_intent")] public required string TruestIntent { get; init; }
    [JsonPropertyName("intent_confidence")] public string IntentConfidence { get; init; } = "medium";
    [JsonPropertyName("phase")] public required string Phase { get; init; }
    [JsonPropertyName("steps")] public List<IntentStep> Steps { get; init; } = [];
    [JsonPropertyName("locked")] public bool Locked { get; init; }
    [JsonPropertyName("trail_hash")] public string? TrailHash { get; init; }
    [JsonPropertyName("evidence")] public List<string> Evidence { get; init; } = [];
}

public sealed record IntentStep
{
    [JsonPropertyName("id")] public required int Id { get; init; }
    [JsonPropertyName("action")] public required string Action { get; init; }
    [JsonPropertyName("target")] public required string Target { get; init; }
}
