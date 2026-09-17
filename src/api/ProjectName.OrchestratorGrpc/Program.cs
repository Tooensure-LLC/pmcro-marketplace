using ProjectName.OrchestratorGrpc;
using ProjectName.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddOllamaClients();
builder.AddPmcroMafRunner();

// Add services to the container.
// TEMP-DEBUG: EnableDetailedErrors surfaces the real exception message/stack in the
// RpcException instead of the generic "Exception was thrown by handler." — remove once
// the post-materializer 500 is root-caused.
builder.Services.AddGrpc(options => options.EnableDetailedErrors = builder.Environment.IsDevelopment());

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrchestratorGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
