namespace PMCR.Core.OMode;

public sealed class OModeSelector
{
    public OModeType Select(string intent, int loopCount)
    {
        if (loopCount >= 2) return OModeType.Patch;
        if (intent.Contains("colony", StringComparison.OrdinalIgnoreCase)) return OModeType.Colony;
        if (intent.Length < 80) return OModeType.MetaFast;
        return OModeType.Standard;
    }
}
