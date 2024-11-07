namespace BloggingPlatform.Data.Entities;

public class BlogPost : BaseEntity
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public string UserId { get; set; } = null!;
    public required User Author{ get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}