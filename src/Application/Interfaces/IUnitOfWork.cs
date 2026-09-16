using Domain.Common;

namespace Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Generic — closed UoW, never need to add properties again
    IGenericRepository<T> Repository<T>() where T : BaseEntity;

    // For custom repos that extend GenericRepository<T>
    TRepository GetRepository<TRepository>() where TRepository : class;

    TRepository GetCustomRepository<T, TRepository>()
        where T : BaseEntity
        where TRepository : class, IGenericRepository<T>;

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
