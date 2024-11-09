using BloggingPlatform.Data.Entities;

namespace BloggingPlatform.Business.Models.Responses;

public class CommentResponse
{
    public string Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; }
    public User Commenter { get; set; }
    public string BlogPostId { get; set; }
}