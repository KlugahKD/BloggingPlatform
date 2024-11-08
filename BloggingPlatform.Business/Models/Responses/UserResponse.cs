namespace BloggingPlatform.Business.Models.Responses;

public record UserResponse
(
    string Id, 
    DateTime CreatedAt ,
    string FullName ,
    string Email,
    string PhoneNumber 
);