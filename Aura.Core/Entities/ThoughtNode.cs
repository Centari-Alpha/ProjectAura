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

    public List<MediaTag> Attachments { get; set; } = new();

    public List<Relationship> Connections { get; set; } = new();

    public void AddConnection(Guid targetId, float strength)
    {
        if (!Connections.Exists(r => r.TargetNodeId == targetId))
        {
            Connections.Add(new Relationship(targetId, strength));
        }
    }

}

public class AuraVector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public AuraVector3() { } // Needed for EF Core
    public AuraVector3(float x, float y, float z) { X = x; Y = y; Z = z; }
}