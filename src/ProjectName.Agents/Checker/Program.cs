
using Checker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PMCR.Core.Trails;
using ProjectName.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.AddOllamaClients();
builder.AddPmcroMafRunner();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<TrailService>();
builder.Services.AddScoped<CheckerAgent>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    _ = scope.ServiceProvider.GetRequiredService<CheckerAgent>();
}

Console.WriteLine("Checker running");
host.Run();
