using BloggingPlatform.Business.Common;
using BloggingPlatform.Business.Helpers;
using BloggingPlatform.Business.Models.BaseModels;
using BloggingPlatform.Business.Models.Requests;
using BloggingPlatform.Business.Models.Responses;
using BloggingPlatform.Business.Services.Interfaces;
using BloggingPlatform.Data.Entities;
using BloggingPlatform.Data.Repositories.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloggingPlatform.Business.Services.Providers;

public class BlogpostService(IGenericRepository<BlogPost> blogPostRepository, ILogger<BlogpostService> logger, IGenericRepository<User> userRepository) : IBlogpostService
{
    public async Task<ServiceResponse<BlogPostResponse>> CreateBlogPostAsync(string userId, BlogPostRequest request)
    {
        try
        {
            logger.LogInformation("User {UserId} creating blog post", userId);
            
            var validRequest = string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content);
            
            if (!validRequest)
            {
                logger.LogDebug("Invalid request. Title and content are required.");
                
                return Response.BadRequestResponse<BlogPostResponse>("Title and content are required");
            }
            
            var user = await userRepository.FindAsync(u => u.Id == userId);
            
            if (user is null)
            {
                logger.LogDebug("User with ID {UserId} not found.", userId);
                
                return Response.NotFoundResponse<BlogPostResponse>("User not found");
            }
            
            var blogPost = new BlogPost
            {
                Id = Guid.NewGuid().ToString("N"),
                Title = request.Title,
                Content = request.Content,
                Author = user,
                UserId = userId,
                CreatedAt = DateTime.Now,
                IsActive = true,
            };

            var isSaved = await blogPostRepository.AddAsync(blogPost);

            if (!isSaved)
            {
                logger.LogError("Error occurred while saving blog post.\r\nBlogPostRequest: {Request}", request);
                
                return Response.FailedDependencyResponse<BlogPostResponse>("Error occurred while saving blog post");
            }

            return Response.OkResponse(blogPost.Adapt<BlogPostResponse>());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating blog post");
            
            return Response.InternalServerErrorResponse<BlogPostResponse>(Constants.Response.InternalServerError);
        }
    }

    public async Task<ServiceResponse<BlogPostResponse>> UpdateBlogPostAsync(string postId, UpdateBlogpostRequest request)
    {
        try
        {
            logger.LogInformation("Updating blog post with ID {PostId}", postId);
            
            var validRequest = string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content);
            
            if (!validRequest)
            {
                logger.LogDebug("Invalid request. Title and content are required.");
                
                return Response.BadRequestResponse<BlogPostResponse>("Title and content are required");
            }
            
            var blogPost = await blogPostRepository.FindAsync(bp => bp.Id == postId);

            if (blogPost is null)
            {
                logger.LogDebug("Blog post with ID {PostId} not found.", postId);
                
                return Response.NotFoundResponse<BlogPostResponse>("Blog post not found");
            }

            blogPost.Title =  request.Title ?? blogPost.Title;
            blogPost.Content = request.Content ?? blogPost.Content;

            var isUpdated = await blogPostRepository.UpdateAsync(blogPost);

            if (!isUpdated)
            {
                logger.LogError("Error occurred while updating blog post.\r\nBlogPostRequest: {Request}", request);
                
                return Response.FailedDependencyResponse<BlogPostResponse>("Error occurred while updating blog post");
            }

            return Response.OkResponse(blogPost.Adapt<BlogPostResponse>());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating blog post with ID {PostId}", postId);
            
            return Response.InternalServerErrorResponse<BlogPostResponse>(Constants.Response.InternalServerError);
        }
    }

    public async Task<ServiceResponse<BlogPostResponse>> DeleteBlogPostAsync(string postId)
    {
        try
        {
            var blogPost = await blogPostRepository.FindAsync(bp => bp.Id == postId);

            if (blogPost is null)
            {
                logger.LogDebug("Blog post with ID {PostId} not found.", postId);
                
                return Response.NotFoundResponse<BlogPostResponse>("Blog post not found");
            }

            var isDeleted = await blogPostRepository.SoftDeleteAsync(postId);

            if (!isDeleted)
            {
                logger.LogError("Error occurred while deleting blog post.\r\n PostId: {PostId}", postId);
                
                return Response.FailedDependencyResponse<BlogPostResponse>("Error occurred while deleting blog post");
            }

            return Response.OkResponse(blogPost.Adapt<BlogPostResponse>());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting blog post with ID {PostId}", postId);
            
            return Response.InternalServerErrorResponse<BlogPostResponse>(Constants.Response.InternalServerError);
        }
    }

    public async Task<ServiceResponse<BlogPostResponse>> GetBlogPostByIdAsync(string postId)
    {
        try
        {
            var blogPost = await blogPostRepository.FindAsync(bp => bp.Id == postId);

            if (blogPost is null)
            {
                logger.LogDebug("Blog post with ID {PostId} not found.", postId);
                
                return Response.NotFoundResponse<BlogPostResponse>("Blog post not found");
            }

            return Response.OkResponse(blogPost.Adapt<BlogPostResponse>());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving blog post with ID {PostId}", postId);
            
            return Response.InternalServerErrorResponse<BlogPostResponse>(Constants.Response.InternalServerError);
        }
    }

    public async Task<ServiceResponse<PagedResult<BlogPostResponse>>> GetAllBlogPostsAsync(BaseFilter filter)
    {
        try
        {
            var query =  blogPostRepository.AsQueryable().Where(u => u.IsActive);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(bp => bp.Title.Contains(filter.Search) || bp.Content.Contains(filter.Search));
            }

            var totalCount = await query.CountAsync();

            var blogPosts = await query
                .OrderByDescending(bp => bp.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(bp => new BlogPostResponse
                {
                    Id = bp.Id,
                    Title = bp.Title,
                    Content = bp.Content,
                    CreatedAt = bp.CreatedAt,
                    Author = bp.Author,
                    UserId = bp.UserId,
                    Comments = bp.Comments,
                })
                .ToListAsync();

            var response = new PagedResult<BlogPostResponse>
            {
                Page = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount,
                Payload = blogPosts,
            };

            return Response.OkResponse(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get all blog posts. {Ex}", e.Message);
            
            return Response.InternalServerErrorResponse<PagedResult<BlogPostResponse>>(Constants.Response.InternalServerError);
        }
    }
}