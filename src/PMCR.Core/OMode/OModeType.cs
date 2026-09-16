namespace PMCR.Core.OMode;

public enum OModeType { Standard, Patch, Colony, MetaFast }

public static class OModeExtensions
{
    public static OModeType Parse(string s) => s.ToLower() switch
    {
        "patch" => OModeType.Patch,
        "colony" => OModeType.Colony,
        "meta-fast" => OModeType.MetaFast,
        _ => OModeType.Standard
    };
}
