using Koi_Web_BE.Models.Primitives;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koi_Web_BE.Models.Entities;

public class Trip : BaseEntity
{
    public Guid FarmId { get; set; }
    public bool? IsApproved { get; set; } = null!;
    public int Days { get; set; } = 1;
    public decimal Price { get; set; }
    [ForeignKey(nameof(FarmId))]
    public Farm Farm { get; set; } = null!;
    public IList<OrderTrip> OrderTrips { get; set; } = [];
}