using System.ComponentModel.DataAnnotations.Schema;
using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class OrderKoi : BaseEntity
{
    public required Guid OrderId { get; set; }
    public required Guid KoiId { get; set; }
    public int Quantity { get; set; } = 1;
    // Relationships
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;
    [ForeignKey(nameof(KoiId))]
    public Koi Koi { get; set; } = null!;
}