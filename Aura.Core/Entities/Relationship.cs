namespace Aura.Core.Entities;

using System;

public class Relationship
{
    public Relationship() { } // Needed for EF Core

    public Relationship(Guid targetId, float strength = 0.5f)
    {
        TargetNodeId = targetId;
        ConnectionStrength = strength;
    }
    
    public Guid SourceNodeId { get; set; }
    public ThoughtNode SourceNode { get; set; }

    public Guid TargetNodeId { get; set; }
    public ThoughtNode TargetNode { get; set; }

    public float ConnectionStrength { get; set; }
    public string Description { get; set; }
}