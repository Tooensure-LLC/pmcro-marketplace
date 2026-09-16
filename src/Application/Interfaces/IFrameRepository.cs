using Domain.Entities;

namespace Application.Interfaces;

public interface IFrameRepository : IGenericRepository<Frame>
{
    Task<Frame?> GetLatestByTrailAndRoleAsync(Guid trailId, string role, CancellationToken ct = default);
    Task<IReadOnlyList<Frame>> GetByTrailAsync(Guid trailId, CancellationToken ct = default);
}
