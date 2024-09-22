using System.ComponentModel.DataAnnotations.Schema;
using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class Color : BaseEntity
{
    public required Guid KoiId { get; set; }
    public required string Name { get; set; }
    // Relations
    [ForeignKey(nameof(KoiId))]
    public Koi Koi { get; set; } = null!;
}