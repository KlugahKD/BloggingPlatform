namespace BloggingPlatform.Business.Models.Requests;

public record UserRequest
{
    public required string FullName { get; set; }
    public string? Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Password { get; set; }
}