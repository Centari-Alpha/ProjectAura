namespace Aura.Core.Interfaces;

using System;
using System.Collections.Generic;
using Aura.Core.Entities;

public interface IAuraGraphRepository
{
    ThoughtNode GetNode(Guid id);
    IEnumerable<ThoughtNode> GetAllNodes();
    void AddNode(ThoughtNode node);
    void UpdateNode(ThoughtNode node);
    void RemoveNode(Guid id);

    void ConnectNodes(Guid sourceId, Guid targetId, float strength, string description = "");
    void DisconnectNodes(Guid sourceId, Guid targetId);
    
    IEnumerable<ThoughtNode> GetNeighbors(Guid nodeId, int maxDepth = 1);
    
    IEnumerable<ThoughtNode> GetNodesInRadius(AuraVector3 centerPoint, float radius);

    void AddConstellation(Constellation constellation);
    void UpdateConstellation(Constellation constellation);
    void MapNodeToConstellation(Guid nodeId, Guid constellationId);
    IEnumerable<Constellation> GetAllConstellations();
}
