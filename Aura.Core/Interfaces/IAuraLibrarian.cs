using System;
using Aura.Core.DTOs;

namespace Aura.Core.Interfaces;

public interface IAuraLibrarian
{
    GraphViewDto GetViewportState();
    GraphViewDto GetClusterView(Guid centerThoughtId, int depth = 2);
}
