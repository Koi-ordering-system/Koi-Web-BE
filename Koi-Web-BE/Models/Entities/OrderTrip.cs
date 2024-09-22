using Koi_Web_BE.Models.Enums;

namespace Koi_Web_BE.Models.Entities;

public class OrderTrip : BaseEntity
{
    public new required Guid Id { get; set; }
    public required DateTimeOffset StartDate { get; set; }
    public required DateTimeOffset EndDate { get; set; }
    public TripStatusEnum Status { get; set; } = TripStatusEnum.Pending;
}