using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TrailRepository : GenericRepository<Trail>, ITrailRepository
{
    public TrailRepository(AppDbContext context) : base(context) {}

    public async Task<Trail?> GetWithFramesAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.Include(t => t.Frames.Where(f => !f.IsDeleted))
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);

    public async Task<IReadOnlyList<Trail>> GetOpenTrailsAsync(CancellationToken ct = default)
        => await _dbSet.Where(t => t.Status == "open" && !t.IsDeleted).ToListAsync(ct);
}
