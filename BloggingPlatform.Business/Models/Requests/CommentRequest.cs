namespace BloggingPlatform.Business.Models.Requests;

public record CommentRequest( string Content, string PostId, string UserId );