using System;
using System.Collections.Generic;
using Aura.Core.Entities;
using Aura.Core.Enums;

namespace Aura.Core.Interfaces;

public interface IAuraGraphService
{
    ThoughtNode CreateThought(string content, NodeEssence essence, float initialWeight = 1.0f);
    void FulfillThought(Guid id, float fulfillmentScore);
    void ConnectThoughts(Guid sourceId, Guid targetId, string description = "");
    
    Constellation CreateConstellation(string name, string theme);
    void AddThoughtToConstellation(Guid thoughtId, Guid constellationId);
    void RecalculateConstellationMetrics(Guid constellationId);
    
    IEnumerable<ThoughtNode> GetThoughtCluster(Guid rootId, int depth = 1);
}
