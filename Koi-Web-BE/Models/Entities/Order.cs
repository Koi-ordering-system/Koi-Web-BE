using Koi_Web_BE.Models.Enums;

namespace Koi_Web_BE.Models.Entities;

public class Order : BaseEntity
{
    public required string UserId { get; set; }
    public decimal Price { get; set; } = 0;
    public required bool IsPaid { get; set; } = false;
    public OrderStatusEnum Status { get; set; } = OrderStatusEnum.Pending;
}