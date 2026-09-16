namespace PMCR.Core.Laws;

public static class EC_004
{
    public const string Code = "EC-004";
    public const string Rule = "Checker Verdict Only — Maker never self-scores. Only Checker can issue PASS/LOOP verdict.";

    public static void EnforceMakerDoesNotScore(string role, string verdict)
    {
        if (role.Equals("maker", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(verdict))
            throw new InvalidOperationException($"{Code}: {Rule} — Maker attempted to issue verdict {verdict}");
    }

    public static void EnforceCheckerVerdict(string role, string verdict)
    {
        if (role.Equals("checker", StringComparison.OrdinalIgnoreCase))
        {
            var allowed = new[] { "PASS", "LOOP", "HALT" };
            if (!allowed.Contains(verdict?.ToUpperInvariant()))
                throw new InvalidOperationException($"{Code}: Invalid Checker verdict {verdict}. Allowed: PASS/LOOP/HALT");
        }
    }
}
