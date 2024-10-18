using System.ComponentModel.DataAnnotations.Schema;
using Koi_Web_BE.Models.Enums;
using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class OrderTrip : BaseEntity
{
    public required Guid OrderId { get; set; }
    public required Guid TripId { get; set; }
    public required DateTimeOffset StartDate { get; set; }
    public required DateTimeOffset EndDate { get; set; }
    public TripStatusEnum Status { get; set; } = TripStatusEnum.Pending;
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;
    [ForeignKey(nameof(TripId))]
    public Trip Trip { get; set; } = null!;
}