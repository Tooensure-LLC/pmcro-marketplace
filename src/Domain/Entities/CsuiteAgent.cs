using Domain.Common;

namespace Domain.Entities;

public class CsuiteAgent : BaseEntity
{
    public string Role { get; set; } = default!; // CEO, CTO, CFO...
    public string DomainOwns { get; set; } = default!;
    public string DoesNotOwn { get; set; } = default!;
    public string SkillPath { get; set; } = default!;
}
