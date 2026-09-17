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

// The static EarnedConstraints class that used to live here (three hardcoded, hand-seeded
// strings: NoHardcodedPaths, NoNewInUoW, TrailSealedImmutable) has been removed. Those three
// were never actually earned by a Reflector promoting a real LOOP -> ACCEPT trail — they were
// invented, which is the opposite of what "earned" is supposed to mean here.
// The single real representation is now Domain.Entities.EarnedConstraint, persisted through
// IUnitOfWork like Trail/Frame. It starts empty and only ReflectorAgent (once implemented,
// see .pmcro/tool-reference.md — currently a stub) may add rows to it, one per genuinely
// earned lesson.
