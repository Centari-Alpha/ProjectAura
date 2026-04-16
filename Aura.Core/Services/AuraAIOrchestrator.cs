using System;
using System.Linq;
using System.Threading.Tasks;
using Aura.Core.Entities;
using Aura.Core.Enums;
using Aura.Core.Interfaces;

namespace Aura.Core.Services;

public class AuraAIOrchestrator : IAuraAIOrchestrator
{
    private readonly IAuraGraphService _graphService;
    private readonly IAuraGraphRepository _repository; // Need direct read access sometimes

    public AuraAIOrchestrator(IAuraGraphService graphService, IAuraGraphRepository repository)
    {
        _graphService = graphService;
        _repository = repository;
    }

    public Task<ThoughtNode> IngestThoughtAsync(string rawInput)
    {
        // 1. Analyze input and determine essence (Stubbed logic for now)
        var estimatedEssence = DetermineEssence(rawInput);
        
        // 2. Create the thought
        var thought = _graphService.CreateThought(rawInput, estimatedEssence);

        // 3. Find related existing thoughts to connect to
        // Example: connect to the most recently active thought or similar essence
        var allNodes = _repository.GetAllNodes().ToList();
        var similarNodes = allNodes
            .Where(n => n.Id != thought.Id && n.Essence == estimatedEssence)
            .OrderByDescending(n => n.CreatedAt)
            .Take(3);

        foreach (var node in similarNodes)
        {
            _graphService.ConnectThoughts(thought.Id, node.Id, "Orchestrator Automated Connection");
        }

        return Task.FromResult(thought);
    }

    public Task OrganizeGraphAsync()
    {
        // Future logic for AI to cluster nodes, form constellations automatically
        // e.g. finding highly connected subgraphs and calling _graphService.CreateConstellation
        
        return Task.CompletedTask;
    }

    private NodeEssence DetermineEssence(string input)
    {
        var lower = input.ToLower();
        if (lower.Contains("feel") || lower.Contains("sad") || lower.Contains("happy")) return NodeEssence.Emotion;
        if (lower.Contains("if") || lower.Contains("then") || lower.Contains("calculate") || lower.Contains("why")) return NodeEssence.Logic;
        if (lower.Contains("remember") || lower.Contains("yesterday")) return NodeEssence.Memory;
        if (lower.Contains("goal") || lower.Contains("want to") || lower.Contains("will")) return NodeEssence.Aspiration;
        return NodeEssence.Philosophy; // Default fallback
    }
}
