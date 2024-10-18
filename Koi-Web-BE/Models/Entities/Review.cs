using System.ComponentModel.DataAnnotations.Schema;
using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class Review : BaseEntity
{
    public required string UserId { get; set; }
    public required Guid OrderId { get; set; }
    public int Rating { get; set; } = 0;
    public string Content { get; set; } = string.Empty;
    //Relations
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;
}