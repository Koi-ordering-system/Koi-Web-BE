using System.ComponentModel.DataAnnotations.Schema;
using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class FarmImage : BaseEntity
{
    public required Guid FarmId { get; set; }
    public required string Url { get; set; }
    [ForeignKey(nameof(FarmId))]
    public Farm Farm { get; set; } = null!;
}