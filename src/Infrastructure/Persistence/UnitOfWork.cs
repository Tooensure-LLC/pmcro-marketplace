using Application.Interfaces;
using Domain.Common;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);
        if (_repositories.TryGetValue(type, out var repo)) return (IGenericRepository<T>)repo;
        var newRepo = new Infrastructure.Persistence.Repositories.GenericRepository<T>(_context);
        _repositories[type] = newRepo;
        return newRepo;
    }

    public TRepository GetRepository<TRepository>() where TRepository : class
    {
        var type = typeof(TRepository);
        if (_repositories.TryGetValue(type, out var repo)) return (TRepository)repo;
        var newRepo = _serviceProvider.GetRequiredService<TRepository>();
        _repositories[type] = newRepo;
        return newRepo;
    }

    public TRepository GetCustomRepository<T, TRepository>() where T : BaseEntity where TRepository : class, IGenericRepository<T>
        => GetRepository<TRepository>();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null) { await _transaction.CommitAsync(ct); await _transaction.DisposeAsync(); _transaction = null; }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null) { await _transaction.RollbackAsync(ct); await _transaction.DisposeAsync(); _transaction = null; }
    }

    public void Dispose() { _transaction?.Dispose(); _context.Dispose(); }
}
