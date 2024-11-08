namespace BloggingPlatform.Business.Models.Requests;

public record UpdateBlogpostRequest(
    string? Title,
    string? Content
    );