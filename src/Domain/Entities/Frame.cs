using Domain.Common;

namespace Domain.Entities;

public class Frame : BaseEntity, IAppendOnlyEntity
{
    public Guid TrailId { get; set; }
    public Trail Trail { get; set; } = default!;
    public string Role { get; set; } = default!; // planner, maker, checker, reflector
    public int CycleNumber { get; set; } = 1;
    public string Verdict { get; set; } = "PENDING";
    public string TypedEnvelopeJson { get; set; } = "{}";
    public string Evidence { get; set; } = string.Empty;
}
