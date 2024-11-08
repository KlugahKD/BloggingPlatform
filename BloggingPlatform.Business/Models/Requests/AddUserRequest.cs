using System.ComponentModel.DataAnnotations;

namespace BloggingPlatform.Business.Models.Requests;

public class AddUserRequest
{
    public required string FullName { get; set; }

    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; } = null!;
    public required string PhoneNumber { get; set; }
    public required string Password { get; set; }
}