namespace Aura.Core.Entities;

public class NodeState
{
    public Guid NodeId { get; set; }
    
    public AuraVector3 CurrentVelocity { get; set; }
    public float CurrentOrbitAngle { get; set;} 
    public bool IsBeingGazedAt { get; set; }

    public DateTime LastInteractionTime { get; set; }
    public float PulseAlpha { get; set; }
}