using System;
using System.Collections.Generic;

namespace Aura.Core.DTOs;

public class GraphViewDto
{
    public List<NodeViewDto> Nodes { get; set; } = new();
    public List<EdgeViewDto> Edges { get; set; } = new();
}

public class NodeViewDto
{
    public Guid Id { get; set; }
    public string? Content { get; set; }
    public string? Essence { get; set; }
    public float Weight { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

public class EdgeViewDto
{
    public Guid SourceId { get; set; }
    public Guid TargetId { get; set; }
    public float Strength { get; set; }
}
