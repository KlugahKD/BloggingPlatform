using BloggingPlatform.Business.Common;
using BloggingPlatform.Business.Models.BaseModels;
using BloggingPlatform.Business.Models.Requests;
using BloggingPlatform.Business.Models.Responses;

namespace BloggingPlatform.Business.Services.Interfaces;

public interface ICommentService
{
    Task<ServiceResponse<CommentResponse>> CreateCommentAsync(CommentRequest request);
    Task<ServiceResponse<CommentResponse>> UpdateCommentAsync(string commentId,string userId, UpdateCommentRequest request);
    Task<ServiceResponse<CommentResponse>> DeleteCommentAsync(string commentId, string userId);
    Task<ServiceResponse<CommentResponse>> GetCommentByIdAsync(string commentId);
    Task<ServiceResponse<PagedResult<CommentResponse>>> GetAllCommentsByBlogPostIdAsync(string blogPostId, BaseFilter filter);
}