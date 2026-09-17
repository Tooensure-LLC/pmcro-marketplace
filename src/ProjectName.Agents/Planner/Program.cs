using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Planner;
using PMCR.Core.Trails;
using ProjectName.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.AddOllamaClients();
builder.AddPmcroMafRunner();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<TrailService>();
builder.Services.AddScoped<PlannerAgent>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    _ = scope.ServiceProvider.GetRequiredService<PlannerAgent>();
}

Console.WriteLine("Planner running");
host.Run();
