namespace Aura.Core.Entities;

using System;
using System.Collections.Generic;
using Aura.Core.Enums;

public class ThoughtNode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public NodeEssence Essence { get; set; }
    public float FulfillmentScore { get; set; }
    public float Weight { get; set; }

    public AuraVector3 DailyHomePosition { get; set; }
    public bool IsOrbiting { get; set; }

    public List<Relationship> Connections { get; set; } = new();

    public void AddConnection(Guid targetId, float strength)
    {
        if (!Connections.Exists(r => r.TargetNodeId == targetId))
        {
            Connections.Add(new Relationship(targetId, strength));
        }
    }

    public struct AuraVector3
    {
        public float X, Y, Z;
        public AuraVector3(float x, float y, float z) { X = x; Y = y; Z = z; }
    }
}