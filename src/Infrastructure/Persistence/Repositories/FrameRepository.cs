using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class FrameRepository : GenericRepository<Frame>, IFrameRepository
{
    public FrameRepository(AppDbContext context) : base(context) {}

    public async Task<Frame?> GetLatestByTrailAndRoleAsync(Guid trailId, string role, CancellationToken ct = default)
        => await _dbSet.Where(f => f.TrailId == trailId && f.Role == role && !f.IsDeleted)
            .OrderByDescending(f => f.CycleNumber).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Frame>> GetByTrailAsync(Guid trailId, CancellationToken ct = default)
        => await _dbSet.Where(f => f.TrailId == trailId && !f.IsDeleted)
            .OrderBy(f => f.CycleNumber).ToListAsync(ct);
}
