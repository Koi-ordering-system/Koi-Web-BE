namespace Koi_Web_BE.Models.Entities;

public class CartItem : BaseEntity
{
    public required Guid CartId { get; set; }
    public required Guid FarmKoiId { get; set; }
    public int Quantity { get; set; } = 1;
}