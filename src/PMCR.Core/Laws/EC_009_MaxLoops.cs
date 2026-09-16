namespace PMCR.Core.Laws;

public static class EC_009
{
    public const string Code = "EC-009";
    public const string Rule = "MaxLoops Enforcement — Trail must halt to human after maxLoops, never infinite loop.";
    public const int DefaultMaxLoops = 3;

    public static void Enforce(int currentLoop, int maxLoops = DefaultMaxLoops)
    {
        if (currentLoop >= maxLoops)
            throw new InvalidOperationException($"{Code}: {Rule} — currentLoop {currentLoop} >= maxLoops {maxLoops}. HALT to human required.");
    }

    public static bool ShouldHalt(int currentLoop, int maxLoops = DefaultMaxLoops) => currentLoop >= maxLoops;
}

public static class PLAN_001
{
    public const string Code = "PLAN-001";
    public const string Rule = "Typed Envelope — Every frame must be schema-valid JSONL, no <placeholder> left.";

    public static void Enforce(string jsonl)
    {
        if (string.IsNullOrWhiteSpace(jsonl) || jsonl.Contains("<placeholder>"))
            throw new InvalidOperationException($"{Code}: {Rule}");
    }
}

public static class EarnedConstraints
{
    // Constraints earned during Reflector phase
    public const string NoHardcodedPaths = "Earned: No hardcoded absolute paths — use Aspire volumes";
    public const string NoNewInUoW = "Earned: UoW stays closed — use Repository<T>() + GetRepository<TRepo>()";
    public const string TrailSealedImmutable = "Earned: Sealed trail is immutable, export to .pmcro/trails for audit";
}
