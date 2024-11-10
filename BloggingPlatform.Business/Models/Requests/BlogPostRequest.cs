namespace BloggingPlatform.Business.Models.Requests;

public class BlogPostRequest
{ 
   public string Title { get; set; }
   public string Content { get; set; }
   
   public List<string> Tags { get; set; } 
}