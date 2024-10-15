using System.Text.Json.Serialization;

namespace Koi_Web_BE.Models.Requests;


// Define the Event class to represent the webhook event
public class Event
{
    public string? Type { get; set; }
    public ClerkUser? Data { get; set; }
}

public class ClerkUser
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("username")]
    public string? Username { get; set; }
    [JsonPropertyName("email_addresses")]
    public List<ClerkEmailAddress> EmailAddresses { get; set; } = [];
    [JsonPropertyName("phone_numbers")]
    public List<ClerkPhoneNumber> PhoneNumbers { get; set; } = [];
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }
}

public class ClerkEmailAddress
{
    [JsonPropertyName("email_address")]
    public string? EmailAddress { get; set; }
}

public class ClerkPhoneNumber
{
    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }
}