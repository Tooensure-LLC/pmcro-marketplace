using Application.Interfaces;
using Domain.Common;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public IQueryable<T> Query() => _dbSet.Where(x => !x.IsDeleted).AsQueryable();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.Where(x => !x.IsDeleted).AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.Where(x => !x.IsDeleted).Where(predicate).AsNoTracking().ToListAsync(ct);

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        await _dbSet.AddAsync(entity, ct);
        return entity;
    }

    public Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        var q = _dbSet.Where(x => !x.IsDeleted);
        if (predicate != null) q = q.Where(predicate);
        return await q.CountAsync(ct);
    }
}
