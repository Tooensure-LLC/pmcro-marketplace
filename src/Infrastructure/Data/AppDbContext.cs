using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Trail> Trails => Set<Trail>();
    public DbSet<Frame> Frames => Set<Frame>();
    public DbSet<CsuiteAgent> CsuiteAgents => Set<CsuiteAgent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trail>().HasMany(t => t.Frames).WithOne(f => f.Trail).HasForeignKey(f => f.TrailId);
        modelBuilder.Entity<Trail>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Frame>().HasQueryFilter(f => !f.IsDeleted);
        base.OnModelCreating(modelBuilder);
    }
}
