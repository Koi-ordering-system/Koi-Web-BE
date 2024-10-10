using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class Farm : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Rating { get; set; } = 0;
    // Relations
    public IList<FarmKoi> FarmKois { get; set; } = [];
    public IList<FarmImage> FarmImages { get; set; } = [];
    public IList<Review> Reviews { get; set; } = [];
    public IList<Order> Orders { get; set; } = [];

    public void Update(string name, string owner, string address, string description, decimal rating)
    {
        Name = name;
        Owner = owner;
        Address = address;
        Description = description;
        Rating = rating;
    }
}