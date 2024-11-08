namespace BloggingPlatform.Business.Models.Requests;

public record AddBlogpostRequest
(
   string Title,
   string Content,
   string UserId
);