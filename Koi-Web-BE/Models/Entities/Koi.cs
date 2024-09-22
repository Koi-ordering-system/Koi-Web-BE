namespace Koi_Web_BE.Models.Entities;

public class Koi : BaseEntity
{
    public required Guid SpeciesId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal MinSize { get; set; } = 0;
    public decimal MaxSize { get; set; } = 0;
    public bool IsMale { get; set; } = true;
    public decimal Price { get; set; } = 0;
}