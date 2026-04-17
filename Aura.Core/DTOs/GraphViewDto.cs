using System;
using System.Collections.Generic;

namespace Aura.Core.DTOs;

[System.Serializable]
public class GraphViewDto
{
    public List<NodeViewDto> Nodes = new();
    public List<EdgeViewDto> Edges = new();
}

[System.Serializable]
public class NodeViewDto
{
    public string Id;
    public string? Content;
    public string? Essence;
    public float Weight;
    public float X;
    public float Y;
    public float Z;
}

[System.Serializable]
public class EdgeViewDto
{
    public string SourceId;
    public string TargetId;
    public float Strength;
}
