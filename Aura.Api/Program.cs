using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Aura.Core.Data;
using Aura.Core.Interfaces;
using Aura.Core.Services;
using Aura.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext wrapper essentially
// Sqlite requires the DB to be created via EnsureCreated / Migration
builder.Services.AddDbContext<AuraDbContext>();

// Register Core Repositories & Services
builder.Services.AddScoped<IAuraGraphRepository, AuraGraphRepository>();
builder.Services.AddScoped<IAuraGraphService, AuraGraphService>();
builder.Services.AddScoped<IAuraAIOrchestrator, AuraAIOrchestrator>();

// Register Workers
builder.Services.AddHostedService<PhysicsEngineWorker>();

var app = builder.Build();

// Ensure DB is created for development purposes
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuraDbContext>();
    dbContext.Database.EnsureCreated();
}

app.MapGet("/", () => "Welcome to Aura API - Your Graph is Alive.");

// Basic Endpoints to interact with Orchestrator
app.MapPost("/api/thought", async (string content, IAuraAIOrchestrator orchestrator) => 
{
    var thought = await orchestrator.IngestThoughtAsync(content);
    return Results.Ok(thought);
});

app.MapGet("/api/graph/nodes", (IAuraGraphRepository repository) => 
{
    var nodes = repository.GetAllNodes();
    return Results.Ok(nodes);
});

app.Run();
