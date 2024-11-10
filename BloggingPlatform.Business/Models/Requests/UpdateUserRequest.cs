namespace BloggingPlatform.Business.Models.Requests;

public record UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
}