namespace Aura.Core.Entities;

using System;

public class Relationship
{
    public Relationship(Guid targetId, float strength = 0.5f)
    {
        TargetNodeId = targetId;
        ConnectionStrength = strength;
    }
    
    public Guid TargetNodeId { get; set; }
    public float ConnectionStrength { get; set; }
    public string Description { get; set; }
}