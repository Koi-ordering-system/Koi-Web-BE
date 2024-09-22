namespace Koi_Web_BE.Models.Entities;

public class FarmImage : BaseEntity
{
    public required Guid FarmId { get; set; }
    public required string Url { get; set; }
}