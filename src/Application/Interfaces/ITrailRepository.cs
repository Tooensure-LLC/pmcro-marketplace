using Domain.Entities;

namespace Application.Interfaces;

public interface ITrailRepository : IGenericRepository<Trail>
{
    Task<Trail?> GetWithFramesAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Trail>> GetOpenTrailsAsync(CancellationToken ct = default);
}
