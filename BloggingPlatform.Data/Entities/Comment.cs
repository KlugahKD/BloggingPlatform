namespace BloggingPlatform.Data.Entities;

public class Comment : BaseEntity
{
    public required string Content { get; set; }

    public required string Commenter { get; set; }
   
    public string BlogPostId { get; set; } 
    
    public BlogPost BlogPost { get; set; } 
}