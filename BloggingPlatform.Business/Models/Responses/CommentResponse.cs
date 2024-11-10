namespace BloggingPlatform.Business.Models.Responses;

public class CommentResponse
{
    public string Id { get; set; }
    public string Content { get; set; }
    public string Commenter { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    public bool IsActive { get; set; }
}