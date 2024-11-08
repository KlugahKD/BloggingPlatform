namespace BloggingPlatform.Business.Models.Responses;

public class UserResponse
{
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public string Role { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? UpdatedAt { get; set; }
}