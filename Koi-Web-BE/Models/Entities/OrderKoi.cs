namespace Koi_Web_BE.Models.Entities;

public class OrderKoi : BaseEntity
{
    public required Guid OrderId { get; set; }
    public required Guid KoiId { get; set; }
    public int Quantity { get; set; } = 1;
}