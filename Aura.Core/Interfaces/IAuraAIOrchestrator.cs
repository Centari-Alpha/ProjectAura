using System;
using System.Linq;
using System.Threading.Tasks;
using Aura.Core.Entities;
using Aura.Core.Enums;
using Aura.Core.Interfaces;

namespace Aura.Core.Interfaces;

public interface IAuraAIOrchestrator
{
    // High-level intent ingestion from the user or "homebrew AI"
    Task<ThoughtNode> IngestThoughtAsync(string rawInput);
    
    // Periodically executed by the system to run intelligence over the graph
    Task OrganizeGraphAsync();
}
