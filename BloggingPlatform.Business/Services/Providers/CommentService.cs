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

public class CommentService(IGenericRepository<Comment> commentRepository,
    IGenericRepository<BlogPost> blogPostRepository,
    IGenericRepository<User> userRepository,
    ILogger<CommentService> logger) : ICommentService
{
    public async Task<ServiceResponse<CommentResponse>> CreateCommentAsync(CommentRequest request, string userId)
    {
        try
        {
            logger.LogInformation("Creating comment");
            
            var user = await userRepository.FindAsync(u => u.Id == userId);
            if (user is null)
            {
                logger.LogDebug("User with ID {UserId} not found.", userId);
                
                return Response.NotFoundResponse<CommentResponse>("User not found");
            }
            
            var blogPost = await blogPostRepository.FindAsync(b => b.Id == request.PostId);
            if (blogPost is null || !blogPost.IsActive)
            {
                logger.LogDebug("Blog post with ID {PostId} not found.", request.PostId);
                
                return Response.NotFoundResponse<CommentResponse>("Blog post not found");
            }
            
            var comment = new Comment
            {
                Id = Guid.NewGuid().ToString("N"),
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Commenter = user.FullName,
                CreatedBy = user.FullName,
                BlogPostId = request.PostId
            };
                
            var isSaved = await commentRepository.AddAsync(comment);

            if (!isSaved)
            {
                logger.LogError("Error occurred while saving comment.\r\nCommentRequest: {Request}", request);
                
                return Response.FailedDependencyResponse<CommentResponse>("Error occurred while saving comment");
            }

            return Response.OkResponse(comment.Adapt<CommentResponse>());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating comment");
            
            return Response.InternalServerErrorResponse<CommentResponse>(Constants.Response.InternalServerError);
        }
    }

    public async Task<ServiceResponse<CommentResponse>> UpdateCommentAsync(string commentId, string userId, UpdateCommentRequest request)
    {
        try
        {
            logger.LogInformation("User with Id {UserId} is updating comment with ID {CommentId}", userId, commentId);
            
            var user = await userRepository.FindAsync(u => u.Id == userId);
            if (user is null)
            {
                logger.LogDebug("User with ID {UserId} not found.", userId);
                
                return Response.NotFoundResponse<CommentResponse>("User not found");
            }
            
            var comment = await commentRepository.FindAsync(c => c.Id == commentId);
            
            if (comment is null || !comment.IsActive)
            {
                logger.LogDebug("Comment with ID {CommentId} not found.", commentId);
                
                return Response.NotFoundResponse<CommentResponse>("Comment not found");
            }
            
            var userMadeTheComment = comment.Commenter == user.FullName;

            if (!userMadeTheComment)
            {
                logger.LogDebug("Comment with ID {CommentId} was not made by user with Id {UserId}.", commentId, userId);
                
                return Response.NotFoundResponse<CommentResponse>("Comment was not made by the user, You cant update it");
            }

            comment.Content = request.Content ?? comment.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            var isUpdated = await commentRepository.UpdateAsync(comment);

            if (!isUpdated)
            {
                logger.LogError("Error occurred while updating comment.\r\nCommentRequest: {Request}", request);
                
                return Response.FailedDependencyResponse<CommentResponse>("Error occurred while updating comment");
            }

            return Response.OkResponse(comment.Adapt<CommentResponse>());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating comment with ID {CommentId}", commentId);
            
            return Response.InternalServerErrorResponse<CommentResponse>(Constants.Response.InternalServerError);
        }
    }

    public async Task<ServiceResponse<CommentResponse>> DeleteCommentAsync(string commentId, string userId)
    {
        try
        {
            logger.LogInformation("Deleting comment with ID {CommentId}", commentId);
        
            var comment = await commentRepository.FindAsync(c => c.Id == commentId);

            if (comment is null || !comment.IsActive)
            {
                logger.LogDebug("Comment with ID {CommentId} not found.", commentId);
                
                return Response.NotFoundResponse<CommentResponse>("Comment not found");
            }
        
            var user = await userRepository.FindAsync(u => u.Id == userId);
            if (user is null)
            {
                logger.LogDebug("User with ID {UserId} not found.", userId);
                
                return Response.NotFoundResponse<CommentResponse>("User not found");
            }

            var userMadeTheComment = comment.Commenter == user.FullName;
            var userIsAdmin = user.Role == "Admin";

            if (!userMadeTheComment && !userIsAdmin)
            {
                logger.LogDebug("User with ID {UserId} is not authorized to delete comment with ID {CommentId}.", userId, commentId);
                
                return Response.ForbiddenResponse<CommentResponse>("User is not authorized to delete this comment");
            }

            var isDeleted = await commentRepository.SoftDeleteAsync(commentId);

            if (!isDeleted)
            {
                logger.LogError("Error occurred while deleting comment.\r\nCommentId: {CommentId}", commentId);
                
                return Response.FailedDependencyResponse<CommentResponse>("Error occurred while deleting comment");
            }

            return Response.OkResponse(comment.Adapt<CommentResponse>());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting comment with ID {CommentId}", commentId);
            
            return Response.InternalServerErrorResponse<CommentResponse>(Constants.Response.InternalServerError);
        }
    }

    public async Task<ServiceResponse<CommentResponse>> GetCommentByIdAsync(string commentId)
    {
        try
        {
            logger.LogInformation("Retrieving comment with ID {CommentId}", commentId);
            
            var comment = await commentRepository.FindAsync(c => c.Id == commentId);

            if (comment is null || !comment.IsActive)
            {
                logger.LogDebug("Comment with ID {CommentId} not found.", commentId);
                
                return Response.NotFoundResponse<CommentResponse>("Comment not found");
            }

            return Response.OkResponse(comment.Adapt<CommentResponse>());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving comment with ID {CommentId}", commentId);
            
            return Response.InternalServerErrorResponse<CommentResponse>(Constants.Response.InternalServerError);
        }
    }

    public async Task<ServiceResponse<PagedResult<CommentResponse>>> GetAllCommentsByBlogPostIdAsync(string blogPostId, BaseFilter filter)
    {
        try
        {
            logger.LogInformation("Retrieving all comments for blog post with ID {BlogPostId}", blogPostId);

            var blogPostExists = await blogPostRepository.ExistsAsync(bp => bp.Id == blogPostId);
            if (!blogPostExists)
            {
                logger.LogDebug("Blog post with ID {BlogPostId} not found.", blogPostId);
                
                return Response.NotFoundResponse<PagedResult<CommentResponse>>("Blog post not found");
            }

            var query = commentRepository.AsQueryable().Where(c => c.IsActive && c.BlogPostId == blogPostId);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(c => c.Content.Contains(filter.Search));
            }

            var totalCount = await query.CountAsync();

            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new CommentResponse
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    Commenter = c.Commenter
                })
                .ToListAsync();

            var response = new PagedResult<CommentResponse>
            {
                Page = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount,
                Payload = comments,
            };

            return Response.OkResponse(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get all comments for blog post. {Ex}", e.Message);
            
            return Response.InternalServerErrorResponse<PagedResult<CommentResponse>>(Constants.Response.InternalServerError);
        }
    }
}