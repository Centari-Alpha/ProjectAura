using System;
using System.Linq;
using System.Collections.Generic;
using Aura.Core.Interfaces;
using Aura.Core.DTOs;

namespace Aura.Core.Services;

public class AuraLibrarian : IAuraLibrarian
{
    private readonly IAuraGraphRepository _repository;

    public AuraLibrarian(IAuraGraphRepository repository)
    {
        _repository = repository;
    }

    public GraphViewDto GetViewportState()
    {
        var allNodes = _repository.GetAllNodes().ToList();
        var graph = new GraphViewDto();

        foreach (var node in allNodes)
        {
            graph.Nodes.Add(new NodeViewDto
            {
                Id = node.Id,
                Content = node.Content,
                Essence = node.Essence.ToString(),
                Weight = node.Weight,
                X = node.DailyHomePosition?.X ?? 0,
                Y = node.DailyHomePosition?.Y ?? 0,
                Z = node.DailyHomePosition?.Z ?? 0
            });

            if (node.Connections != null)
            {
                foreach (var connection in node.Connections)
                {
                    graph.Edges.Add(new EdgeViewDto
                    {
                        SourceId = connection.SourceNodeId,
                        TargetId = connection.TargetNodeId,
                        Strength = connection.ConnectionStrength
                    });
                }
            }
        }

        return graph;
    }

    public GraphViewDto GetClusterView(Guid centerThoughtId, int depth = 2)
    {
        var clusterNodes = _repository.GetNeighbors(centerThoughtId, depth).ToList();
        
        // Always ensure the center node is included even if it has no neighbors yet
        if (!clusterNodes.Any(n => n.Id == centerThoughtId))
        {
            var centerNode = _repository.GetNode(centerThoughtId);
            if (centerNode != null) clusterNodes.Add(centerNode);
        }

        var graph = new GraphViewDto();
        var edgeCache = new HashSet<string>();

        foreach (var node in clusterNodes)
        {
            graph.Nodes.Add(new NodeViewDto
            {
                Id = node.Id,
                Content = node.Content,
                Essence = node.Essence.ToString(),
                Weight = node.Weight,
                X = node.DailyHomePosition?.X ?? 0,
                Y = node.DailyHomePosition?.Y ?? 0,
                Z = node.DailyHomePosition?.Z ?? 0
            });

            if (node.Connections != null)
            {
                foreach (var connection in node.Connections)
                {
                    if (clusterNodes.Any(n => n.Id == connection.TargetNodeId))
                    {
                        var edgeKey = $"{connection.SourceNodeId}_{connection.TargetNodeId}";
                        if (edgeCache.Add(edgeKey))
                        {
                             graph.Edges.Add(new EdgeViewDto
                             {
                                 SourceId = connection.SourceNodeId,
                                 TargetId = connection.TargetNodeId,
                                 Strength = connection.ConnectionStrength
                             });
                        }
                    }
                }
            }
        }

        return graph;
    }
}
