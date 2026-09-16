using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(config.GetConnectionString("pmcro-db")));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ITrailRepository, TrailRepository>();
        services.AddScoped<IFrameRepository, FrameRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
