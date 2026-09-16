using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PMCR.Agents.Reflector;
using PMCR.Core.Trails;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<TrailService>();
builder.Services.AddScoped<ReflectorAgent>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    _ = scope.ServiceProvider.GetRequiredService<ReflectorAgent>();
}

Console.WriteLine("Reflector running");
host.Run();
