using BloggingPlatform.Data.Entities;

namespace BloggingPlatform.Business.Models.Responses;

public class BlogPostResponse
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public User Author { get; set; }
    public ICollection<Comment> Comments { get; set; }
}