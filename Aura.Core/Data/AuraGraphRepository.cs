using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Core.Entities;
using Aura.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aura.Core.Data;

public class AuraGraphRepository : IAuraGraphRepository
{
    private readonly AuraDbContext _context;

    public AuraGraphRepository(AuraDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void AddNode(ThoughtNode node)
    {
        _context.ThoughtNodes.Add(node);
        _context.SaveChanges();
    }

    public void ConnectNodes(Guid sourceId, Guid targetId, float strength, string description = "")
    {
        // Check if relationship already exists
        var exists = _context.Relationships.Any(r => r.SourceNodeId == sourceId && r.TargetNodeId == targetId);
        if (exists) return;

        var relationship = new Relationship(targetId, strength)
        {
            SourceNodeId = sourceId,
            Description = description
        };

        _context.Relationships.Add(relationship);
        _context.SaveChanges();
    }

    public void DisconnectNodes(Guid sourceId, Guid targetId)
    {
        var rel = _context.Relationships
            .FirstOrDefault(r => r.SourceNodeId == sourceId && r.TargetNodeId == targetId);
            
        if (rel != null)
        {
            _context.Relationships.Remove(rel);
            _context.SaveChanges();
        }
    }

    public IEnumerable<ThoughtNode> GetAllNodes()
    {
        return _context.ThoughtNodes
            .Include(n => n.Connections)
            .ToList();
    }

    public IEnumerable<ThoughtNode> GetNeighbors(Guid nodeId, int maxDepth = 1)
    {
        if (maxDepth < 1) return Enumerable.Empty<ThoughtNode>();

        var visited = new HashSet<Guid> { nodeId };
        var queue = new Queue<(Guid NodeId, int Depth)>();
        var result = new List<ThoughtNode>();

        queue.Enqueue((nodeId, 0));

        while (queue.Count > 0)
        {
            var (currentId, currentDepth) = queue.Dequeue();

            if (currentDepth >= maxDepth) continue;

            // Get outgoing edges
            var outgoingNodeId = _context.Relationships
                .Where(r => r.SourceNodeId == currentId)
                .Select(r => r.TargetNodeId)
                .ToList();

            // Get incoming edges
            var incomingNodeId = _context.Relationships
                .Where(r => r.TargetNodeId == currentId)
                .Select(r => r.SourceNodeId)
                .ToList();

            var neighborIds = outgoingNodeId.Concat(incomingNodeId).Distinct();

            foreach (var nId in neighborIds)
            {
                if (visited.Add(nId))
                {
                    var neighborNode = _context.ThoughtNodes
                        .Include(n => n.Connections)
                        .FirstOrDefault(n => n.Id == nId);
                        
                    if (neighborNode != null)
                    {
                        result.Add(neighborNode);
                        queue.Enqueue((nId, currentDepth + 1));
                    }
                }
            }
        }

        return result;
    }

    public ThoughtNode GetNode(Guid id)
    {
        return _context.ThoughtNodes
            .Include(n => n.Connections)
            .FirstOrDefault(n => n.Id == id);
    }

    public IEnumerable<ThoughtNode> GetNodesInRadius(AuraVector3 centerPoint, float radius)
    {
        if (centerPoint == null) return Enumerable.Empty<ThoughtNode>();

        var squaredRadius = radius * radius;
        
        // Using AsEnumerable because EF Core with SQLite might struggle translating complex Math functions
        return _context.ThoughtNodes
            .Include(n => n.Connections)
            .AsEnumerable()
            .Where(n => n.DailyHomePosition != null &&
                Math.Pow(n.DailyHomePosition.X - centerPoint.X, 2) +
                Math.Pow(n.DailyHomePosition.Y - centerPoint.Y, 2) +
                Math.Pow(n.DailyHomePosition.Z - centerPoint.Z, 2) <= squaredRadius)
            .ToList();
    }

    public void RemoveNode(Guid id)
    {
        var node = _context.ThoughtNodes.Find(id);
        if (node != null)
        {
            _context.ThoughtNodes.Remove(node);
            _context.SaveChanges();
        }
    }

    public void UpdateNode(ThoughtNode node)
    {
        _context.ThoughtNodes.Update(node);
        _context.SaveChanges();
    }

    public void AddConstellation(Constellation constellation)
    {
        _context.Constellations.Add(constellation);
        _context.SaveChanges();
    }

    public IEnumerable<Constellation> GetAllConstellations()
    {
        return _context.Constellations.ToList();
    }

    public void MapNodeToConstellation(Guid nodeId, Guid constellationId)
    {
        var constellation = _context.Constellations.Find(constellationId);
        if (constellation != null)
        {
            // Adding nodeId to Constellation if it doesn't already exist
            if (!constellation.NodeIds.Contains(nodeId))
            {
                // Creating a new list so EF core recognizes the change since it's JSON serialization backed
                var newList = new List<Guid>(constellation.NodeIds) { nodeId };
                constellation.NodeIds = newList;
                
                _context.Constellations.Update(constellation);
                _context.SaveChanges();
            }
        }
    }
}
