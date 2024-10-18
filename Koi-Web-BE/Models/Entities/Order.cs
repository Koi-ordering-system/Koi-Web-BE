using System.ComponentModel.DataAnnotations.Schema;
using Koi_Web_BE.Models.Enums;
using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class Order : BaseAuditableEntity
{
    public required string UserId { get; set; }
    public long? PayOSOrderCode { get; set; }
    public required Guid FarmId { get; set; }
    public decimal Price { get; set; } = 0;
    public decimal PrePaidPrice { get; set; } = 0;
    public required bool IsPaid { get; set; } = false;
    public DateTimeOffset? ReceivedAt { get; set; } = null!;
    public OrderStatusEnum? Status { get; set; } = null!;
    // Relations
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
    [ForeignKey(nameof(FarmId))]
    public Farm Farm { get; set; } = null!;
    public OrderTrip OrderTrip { get; set; } = null!;
    public IList<OrderKoi> OrderKois { get; set; } = [];
}