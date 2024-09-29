namespace Koi_Web_BE.Models.Primitives;

public class BaseAuditableEntity : BaseEntity
{
    public required DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; } = null!;
}