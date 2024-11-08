namespace BloggingPlatform.Business.Models.Requests;

public record UpdateUserRequest(
    string? FullName, 
    string? Email, 
    string? PhoneNumber,
    string? Role
    );