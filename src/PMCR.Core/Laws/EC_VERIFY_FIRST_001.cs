namespace PMCR.Core.Laws;

public static class EC_VERIFY_FIRST_001
{
    public const string Code = "EC-VERIFY-FIRST-001";
    public const string Rule = "Verify First — Maker must cite real evidence (command output, diffs, file existence) not claims. No placeholder left.";

    public static void Enforce(string evidence)
    {
        if (string.IsNullOrWhiteSpace(evidence) || evidence.Contains("<placeholder>", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"{Code}: {Rule} — evidence missing or contains placeholder.");
    }

    public static bool IsValid(string typedEnvelopeJson)
    {
        return !string.IsNullOrWhiteSpace(typedEnvelopeJson) 
            && !typedEnvelopeJson.Contains("<placeholder>")
            && !typedEnvelopeJson.Contains("TODO");
    }
}
