namespace Aura.Core.Entities;

using System;
using System.Collections.Generic;
using Aura.Core.Enums;

public class Constellation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Theme { get; set; }
    
    public NodeEssence DominantEssence { get; set; }
    
    public AuraVector3 CenterOfMass { get; set; }
    
    public List<Guid> NodeIds { get; set; } = new();
    
    public float OverallCohesionScore { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}