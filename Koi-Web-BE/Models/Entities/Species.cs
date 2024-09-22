using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class Species : BaseEntity
{
    public required string Name { get; set; }
    // Relations
    public IList<Koi> Kois { get; set; } = [];
}