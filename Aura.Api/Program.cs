using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Aura.Core.Data;
using Aura.Core.Interfaces;
using Aura.Core.Services;
using Aura.Core.DTOs;
using Aura.Api.Workers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Enable CORS so the Unity client can request data from localhost
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUnity", defaultBuilder =>
    {
        defaultBuilder.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
    });
});

// Avoid infinite JSON loops just in case, though Librarian handles this via DTOs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.IncludeFields = true;
    options.SerializerOptions.PropertyNamingPolicy = null; // Forces exact C# casing (PascalCase)
});

// Swagger documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AuraDbContext>();

// Core Services
builder.Services.AddScoped<IAuraGraphRepository, AuraGraphRepository>();
builder.Services.AddScoped<IAuraGraphService, AuraGraphService>();
builder.Services.AddScoped<IAuraAIOrchestrator, AuraAIOrchestrator>();
builder.Services.AddScoped<IAuraLibrarian, AuraLibrarian>();

// Background Worker
builder.Services.AddHostedService<PhysicsEngineWorker>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowUnity");

// Ensure DB is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuraDbContext>();
    dbContext.Database.EnsureCreated();
}

app.MapGet("/", () => "Welcome to Aura API - Your Graph is Alive.");

app.MapPost("/api/thought", async (string content, IAuraAIOrchestrator orchestrator) => 
{
    var thought = await orchestrator.IngestThoughtAsync(content);
    return Results.Ok(thought);
});

// The pure data view
app.MapGet("/api/graph/nodes", (IAuraGraphRepository repository) => 
{
    var nodes = repository.GetAllNodes();
    return Results.Ok(nodes);
});

// The LIBRARIAN views mapped for Unity
app.MapGet("/api/librarian/viewport", (IAuraLibrarian librarian) => 
{
    return Results.Ok(librarian.GetViewportState());
});

app.MapGet("/api/librarian/cluster/{id:guid}", (Guid id, IAuraLibrarian librarian) => 
{
    return Results.Ok(librarian.GetClusterView(id, depth: 2));
});

app.Run();
