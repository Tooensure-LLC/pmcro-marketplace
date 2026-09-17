using Domain.Common;

namespace Domain.Entities;

public enum EarnedConstraintStatus { Candidate, Promoted, Suspended, Retired }

// Reconciles the decision parking-lot.md explicitly flagged and asked not to be made silently:
// this adopts pmcro-runtime's lifecycle shape (candidate -> promoted -> suspended -> retired,
// "update is approval, delete is deny / retire don't erase") as the real representation, even
// though pmcro-runtime's own code is gone. A row starts life as Candidate on the Reflector's
// first promotion off a real LOOP->ACCEPT trail; Promoted only after enough consecutive Checker
// PASS verdicts on the same (SkillId, ScriptId) pair to trust it unattended; any single HALT or
// LOOP on a Promoted pair demotes it back to Suspended, not deleted. Only a Promoted row may be
// read by an AutoApprovalRules predicate — see .pmcro/parking-lot.md's
// "Open reconciliation with pmcro-runtime" and "Autonomous-in-the-loop" sections.
public class EarnedConstraint : BaseEntity, IAppendOnlyEntity
{
    public Guid TrailId { get; set; }
    public Trail Trail { get; set; } = default!;
    public string Rule { get; set; } = default!;
    public EarnedConstraintStatus Status { get; set; } = EarnedConstraintStatus.Candidate;

    // Scope for auto-approval: which skill+script pair this constraint governs. Null for a
    // constraint that isn't scoped to unattended execution (e.g. a general coding-style lesson).
    public string? SkillId { get; set; }
    public string? ScriptId { get; set; }

    public int ConsecutivePassCount { get; set; } = 0;
    public DateTime? PromotedAt { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public string? SuspendedReason { get; set; }
}
