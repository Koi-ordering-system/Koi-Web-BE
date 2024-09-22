using System.ComponentModel.DataAnnotations.Schema;
using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class CartItem : BaseEntity
{
    public required Guid CartId { get; set; }
    public required Guid FarmKoiId { get; set; }
    public int Quantity { get; set; } = 1;
    // Relations
    [ForeignKey(nameof(CartId))]
    public Cart Cart { get; set; } = null!;
    [ForeignKey(nameof(FarmKoiId))]
    public FarmKoi FarmKoi { get; set; } = null!;
}