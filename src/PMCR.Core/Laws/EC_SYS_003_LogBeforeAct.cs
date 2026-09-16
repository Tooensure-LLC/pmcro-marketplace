namespace PMCR.Core.Laws;

public static class EC_SYS_003
{
    public const string Code = "EC-SYS-003";
    public const string Rule = "Log Before Act — Trail frame must exist before any state mutation.";
    public static void Enforce(bool frameExists)
    {
        if (!frameExists) throw new InvalidOperationException($"{Code}: {Rule}");
    }
}
