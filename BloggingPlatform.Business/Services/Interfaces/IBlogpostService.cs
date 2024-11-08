using BloggingPlatform.Business.Common;
using BloggingPlatform.Business.Models.BaseModels;
using BloggingPlatform.Business.Models.Requests;
using BloggingPlatform.Business.Models.Responses;

namespace BloggingPlatform.Business.Services.Interfaces;

public interface IBlogpostService
{
    Task<ServiceResponse<BlogPostResponse>> CreateBlogPostAsync(string userId, BlogPostRequest request);
    Task<ServiceResponse<BlogPostResponse>> UpdateBlogPostAsync(string postId, UpdateBlogpostRequest request);
    Task<ServiceResponse<BlogPostResponse>> DeleteBlogPostAsync(string postId);
    Task<ServiceResponse<BlogPostResponse>> GetBlogPostByIdAsync(string postId);
    Task<ServiceResponse<PagedResult<BlogPostResponse>>> GetAllBlogPostsAsync(BaseFilter filter);
}