namespace Koi_Web_BE.Models.Entities;

public class Farm : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Rating { get; set; } = 0;
}