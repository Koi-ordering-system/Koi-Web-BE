using System.ComponentModel.DataAnnotations.Schema;
using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class FarmKoi : BaseEntity
{
    public required Guid FarmId { get; set; }
    public required Guid KoiId { get; set; }
    // Relations
    [ForeignKey(nameof(FarmId))]
    public Farm Farm { get; set; } = null!;
    [ForeignKey(nameof(KoiId))]
    public Koi Koi { get; set; } = null!;
}