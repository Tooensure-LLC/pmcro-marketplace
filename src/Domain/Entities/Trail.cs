using Domain.Common;

namespace Domain.Entities;

public class Trail : BaseEntity
{
    public string SourceType { get; set; } = "pmcro";
    public Guid CycleId { get; set; } = Guid.NewGuid();
    public string Status { get; set; } = "open";
    public int LoopCount { get; set; } = 0;
    public string Intent { get; set; } = default!;
    public ICollection<Frame> Frames { get; set; } = new List<Frame>();
}
