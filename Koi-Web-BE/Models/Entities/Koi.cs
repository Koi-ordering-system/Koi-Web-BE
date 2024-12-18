using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class Koi : BaseEntity
{
    // public required Guid SpeciesId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal MinSize { get; set; } = 0;
    public decimal MaxSize { get; set; } = 0;
    public decimal Price { get; set; } = 0;
    // Relations
    // [ForeignKey(nameof(SpeciesId))]
    // public Species Species { get; set; } = null!;
    public IList<FarmKoi> FarmKois { get; set; } = [];
    public IList<Color> Colors { get; set; } = [];
    public IList<KoiImage> Images { get; set; } = [];
    public IList<OrderKoi> OrderKois { get; set; } = [];
}