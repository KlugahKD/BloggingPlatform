using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BloggingPlatform.Business.Models.Requests;
using BloggingPlatform.Business.Services.Interfaces;
using BloggingPlatform.API.Helper;
using BloggingPlatform.Business.Models.BaseModels;

namespace BloggingPlatform.API.Controllers;

/// <summary>
/// Endpoint to manage comments
/// </summary>
/// <param name="commentService"></param>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CommentsController(ICommentService commentService) : ControllerBase
{
   

    /// <summary>
    /// Creates a new comment
    /// </summary>
    /// <param name="request">Comment request object</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CommentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token.");
        }
        
        var response = await commentService.CreateCommentAsync(request, userId);
        
        return ActionResultHelper.ToActionResult(response);
    }

    /// <summary>
    /// Updates an existing comment
    /// </summary>
    /// <param name="commentId">Comment ID</param>
    /// <param name="request">Comment request object</param>
    /// <returns></returns>
    [HttpPut("{commentId}")]
    public async Task<IActionResult> UpdateComment(string commentId, [FromBody] UpdateCommentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token.");
        }
        
        var response = await commentService.UpdateCommentAsync(commentId,  userId, request);
        
        return ActionResultHelper.ToActionResult(response);
    }

    /// <summary>
    /// Deletes a comment
    /// </summary>
    /// <param name="commentId">Comment ID</param>
    /// <returns></returns>
    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(string commentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token.");
        }
        
        var response = await commentService.DeleteCommentAsync(commentId , userId);
        
        return ActionResultHelper.ToActionResult(response);
    }

    /// <summary>
    /// Gets a comment by ID
    /// </summary>
    /// <param name="commentId">Comment ID</param>
    /// <returns></returns>
    [HttpGet("{commentId}")]
    public async Task<IActionResult> GetCommentById(string commentId)
    {
        var response = await commentService.GetCommentByIdAsync(commentId);
        
        return ActionResultHelper.ToActionResult(response);
    }

    /// <summary>
    /// Gets all comments for a blog post with pagination and filtering
    /// </summary>
    /// <param name="blogPostId">Blog post ID</param>
    /// <param name="filter">Filter object</param>
    /// <returns></returns>
    [HttpGet("blog-post/{blogPostId}")]
    public async Task<IActionResult> GetAllCommentsByBlogPostId(string blogPostId, [FromQuery] BaseFilter filter)
    {
        var response = await commentService.GetAllCommentsByBlogPostIdAsync(blogPostId, filter);
        
        return ActionResultHelper.ToActionResult(response);
    }
    
}