using BloggingPlatform.Data.Entities;

namespace BloggingPlatform.Business.Models.Responses;

public record CommentResponse(
    string Content, 
    BlogPost BlogPost, 
    User Commenter, 
    DateTime CreatedAt, 
    DateTime UpdatedAt
    );