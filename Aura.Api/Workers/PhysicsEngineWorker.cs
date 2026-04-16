using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Aura.Core.Interfaces;
using System.Linq;
using Aura.Core.Entities;

namespace Aura.Api.Workers;

public class PhysicsEngineWorker : BackgroundService
{
    private readonly ILogger<PhysicsEngineWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PhysicsEngineWorker(ILogger<PhysicsEngineWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Aura Physics Engine simulation started. Space-time is bending...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // We use a scope because our Repository relies on a Scoped DbContext
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<IAuraGraphRepository>();
                    
                    var nodes = repository.GetAllNodes().ToList();
                    var nodesUpdated = 0;
                    
                    foreach(var node in nodes) 
                    {
                         // TODO: Full implementation of N-body physics (Repulsion + Spring forces)
                         // For now, if a node is part of a constellation, drift it toward the center of mass.
                         if (node.DailyHomePosition != null) 
                         {
                             // Drift logic stub
                             /*
                             node.DailyHomePosition.X += 0.01f;
                             repository.UpdateNode(node);
                             nodesUpdated++;
                             */
                         }
                    }
                    
                    if (nodesUpdated > 0) 
                    {
                        _logger.LogDebug($"Physics tick completed. Updated {nodesUpdated} node positions.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred pulling structural gravity forces.");
            }

            await Task.Delay(2000, stoppingToken); // 1 tick every 2 seconds for performance
        }
    }
}
