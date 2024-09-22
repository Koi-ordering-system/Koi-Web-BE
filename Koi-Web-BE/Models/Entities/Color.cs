namespace Koi_Web_BE.Models.Entities;

public class Color : BaseEntity
{
    public required Guid KoiId { get; set; }
    public required string Name { get; set; }
}