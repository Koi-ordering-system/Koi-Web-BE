namespace Koi_Web_BE.Models.Entities;

public class Review : BaseEntity
{
    public required string UserId { get; set; }
    public required Guid FarmId { get; set; }
    public int Rating { get; set; } = 0;
    public string Content { get; set; } = string.Empty;
}