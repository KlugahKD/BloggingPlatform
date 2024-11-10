using BloggingPlatform.Data.Entities;

namespace BloggingPlatform.Business.Models.Responses;

public class BlogPostResponse
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Author { get; set; }
    public ICollection<CommentResponse> Comments { get; set; }

    public List<string> Tags { get; set; }
}