namespace BloggingPlatform.Data.Entities;

public class BlogPost : BaseEntity
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string Author { get; set; }
    public List<Comment> Comments { get; set; } = new();
    public List<string> Tags { get; set; } = new List<string>();
}