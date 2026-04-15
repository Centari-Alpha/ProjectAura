namespace Aura.Core.Entities;

using System;
using System.Collections.Generic;
using Aura.Core.Enums;

public class Constellation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Theme { get; set; }
    
    // Derived overall essence based on the majority of nodes in it, can dictate the hue of the constellation cloud
    public NodeEssence DominantEssence { get; set; }
    
    // Group Center of Mass (useful for VR physics bounds and camera focusing)
    public AuraVector3 CenterOfMass { get; set; }
    
    // The thoughts bounded within this constellation
    public List<Guid> NodeIds { get; set; } = new();
    
    // How tightly clustered they are (can influence the particle density or gravity strength)
    public float OverallCohesionScore { get; set; } 
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
