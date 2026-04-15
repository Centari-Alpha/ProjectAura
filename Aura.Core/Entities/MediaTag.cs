namespace Aura.Core.Entities;

public class MediaTag
{
    public string Provider { get; set; } // e.g., "Spotify", "LocalPhotos", "YouTube"
    public string ResourceId { get; set; } // URL or URI
    public string DisplayName { get; set; }
}