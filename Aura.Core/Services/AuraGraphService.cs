using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Core.Entities;
using Aura.Core.Enums;
using Aura.Core.Interfaces;

namespace Aura.Core.Services;

public class AuraGraphService : IAuraGraphService
{
    private readonly IAuraGraphRepository _repository;

    public AuraGraphService(IAuraGraphRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public ThoughtNode CreateThought(string content, NodeEssence essence, float initialWeight = 1.0f)
    {
        var node = new ThoughtNode
        {
            Content = content,
            Essence = essence,
            Weight = initialWeight,
            DailyHomePosition = new AuraVector3(0, 0, 0), // Default origin initially
            IsOrbiting = false,
            FulfillmentScore = 0f
        };
        
        _repository.AddNode(node);
        return node;
    }

    public void FulfillThought(Guid id, float fulfillmentScore)
    {
        var node = _repository.GetNode(id);
        if (node != null)
        {
            node.FulfillmentScore = fulfillmentScore;
            // Thought becomes "heavier" or "lighter" depending on fulfillment score
            // Here as a default rule we increase the weight based on a positive score.
            node.Weight += fulfillmentScore * 0.1f;
            
            _repository.UpdateNode(node);
        }
    }

    public void ConnectThoughts(Guid sourceId, Guid targetId, string description = "")
    {
        var source = _repository.GetNode(sourceId);
        var target = _repository.GetNode(targetId);

        if (source != null && target != null)
        {
            // Default edge calculation business rule:
            // Connections between identical essences carry more natural strength.
            float strength = source.Essence == target.Essence ? 0.8f : 0.4f;
            _repository.ConnectNodes(sourceId, targetId, strength, description);
        }
    }

    public Constellation CreateConstellation(string name, string theme)
    {
        var constellation = new Constellation
        {
            Name = name,
            Theme = theme,
            CenterOfMass = new AuraVector3(0, 0, 0),
            OverallCohesionScore = 0f
        };
        
        _repository.AddConstellation(constellation);
        return constellation;
    }

    public void AddThoughtToConstellation(Guid thoughtId, Guid constellationId)
    {
        _repository.MapNodeToConstellation(thoughtId, constellationId);
        RecalculateConstellationMetrics(constellationId);
    }

    public void RecalculateConstellationMetrics(Guid constellationId)
    {
        var constellations = _repository.GetAllConstellations();
        var constellation = constellations.FirstOrDefault(c => c.Id == constellationId);
        
        if (constellation != null && constellation.NodeIds.Count > 0)
        {
            float sumX = 0, sumY = 0, sumZ = 0;
            int validNodesCount = 0;
            
            foreach (var nodeId in constellation.NodeIds)
            {
                var node = _repository.GetNode(nodeId);
                if (node != null && node.DailyHomePosition != null)
                {
                    sumX += node.DailyHomePosition.X;
                    sumY += node.DailyHomePosition.Y;
                    sumZ += node.DailyHomePosition.Z;
                    validNodesCount++;
                }
            }

            if (validNodesCount > 0)
            {
                // Calculate geometric center of mass
                constellation.CenterOfMass = new AuraVector3(
                    sumX / validNodesCount, 
                    sumY / validNodesCount, 
                    sumZ / validNodesCount
                );
                
                // Base score off how many nodes are grouped together
                // More valid nodes loosely implies higher cohesion right now.
                constellation.OverallCohesionScore = Math.Min(100f, validNodesCount * 5.0f);
                
                _repository.UpdateConstellation(constellation);
            }
        }
    }

    public IEnumerable<ThoughtNode> GetThoughtCluster(Guid rootId, int depth = 1)
    {
        return _repository.GetNeighbors(rootId, depth);
    }
}
