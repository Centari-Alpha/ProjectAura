namespace Aura.Core.Interfaces;

using System;
using System.Collections.Generic;
using Aura.Core.Entities;

public interface IAuraGraphRepository
{
    // Node Operations
    ThoughtNode GetNode(Guid id);
    IEnumerable<ThoughtNode> GetAllNodes();
    void AddNode(ThoughtNode node);
    void UpdateNode(ThoughtNode node);
    void RemoveNode(Guid id);

    // Connection / Edge Operations
    void ConnectNodes(Guid sourceId, Guid targetId, float strength, string description = "");
    void DisconnectNodes(Guid sourceId, Guid targetId);
    
    // Advanced Graph & Spatial Queries
    IEnumerable<ThoughtNode> GetNeighbors(Guid nodeId, int maxDepth = 1);
    
    // Finding nodes near a coordinate in your VR space
    IEnumerable<ThoughtNode> GetNodesInRadius(AuraVector3 centerPoint, float radius);

    // Constellation Operations
    void AddConstellation(Constellation constellation);
    void MapNodeToConstellation(Guid nodeId, Guid constellationId);
    IEnumerable<Constellation> GetAllConstellations();
}
