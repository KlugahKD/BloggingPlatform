using BloggingPlatform.Data.Entities;

namespace BloggingPlatform.Business.Models.Responses;

public record BlogpostResponse
(
    string Id, 
    string Title, 
    string Content, 
    string UserId, 
    DateTime CreatedAt, 
    User Author
);