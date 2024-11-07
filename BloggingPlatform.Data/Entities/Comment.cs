namespace BloggingPlatform.Data.Entities;

public class Comment : BaseEntity
{
    public required string Content { get; set; }
    public string UserId { get; set; } = null!;
    public required User Commenter { get; set; }
    public string BlogPostId { get; set; } = null!;
    public required BlogPost BlogPost { get; set; }
}