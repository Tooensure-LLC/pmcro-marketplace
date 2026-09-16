namespace Domain.Common;

/// <summary>
/// Base for records that are evidence: written once, never updated, never
/// deleted. A correction is a NEW record referencing the one it corrects.
/// </summary>
/// <remarks>
/// <para>
/// MIGRATION NOTE - this type is new and nothing uses it yet. Entities/Frame.cs
/// currently derives from <see cref="BaseEntity"/>, which carries
/// <c>UpdatedAt</c>, and <c>IGenericRepository&lt;T&gt; where T : BaseEntity</c>
/// hands every derived type <c>UpdateAsync</c> and <c>DeleteAsync</c>. A trail
/// frame is therefore mutable and deletable by type, and the append-only rule
/// survives only as a habit.
/// </para>
/// <para>
/// The fix is to move Frame onto this base and give it an
/// <c>IAppendOnlyRepository&lt;T&gt;</c> that has no update method to call - not
/// "you should not update a frame", but no method that does it. That is a code
/// change to Frame, Application/Interfaces and Infrastructure/Persistence, so it
/// is its own cycle rather than something slipped into a build fix.
/// </para>
/// <para>
/// EF Core is one client of this database among several, so the type system is
/// the first line and not the last. Back it at the storage layer in the same
/// migration: <c>REVOKE UPDATE, DELETE ON frames FROM &lt;app_role&gt;;</c>
/// </para>
/// </remarks>
public abstract class AppendOnlyEntity
{
    /// <summary>Stable identity, assigned at construction.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>When the record was written, UTC. There is no "changed" time.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
