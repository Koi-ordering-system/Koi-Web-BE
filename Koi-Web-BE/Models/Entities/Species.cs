using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class Species : BaseAuditableEntity
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public int YearOfDiscovery { get; set; } = 0;
    public string DiscoveredBy { get; set; } = string.Empty;
    // Relations
    // public IList<Koi> Kois { get; set; } = [];
}