using System.Security.Claims;
using BloggingPlatform.API.Helper;
using Microsoft.AspNetCore.Mvc;
using BloggingPlatform.Business.Models.BaseModels;
using BloggingPlatform.Business.Models.Requests;
using BloggingPlatform.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BloggingPlatform.API.Controllers;

/// <summary>
/// Allows users and admins to create, update, delete and view blog posts
/// </summary>
/// <param name="blogpostService"></param>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]

public class BlogPostsController(IBlogpostService blogpostService) : ControllerBase
{
    /// <summary>
    /// Creates a new blog post
    /// </summary>
    /// <param name="request">Blog post request object</param>
    /// <returns></returns>
[HttpPost]
public async Task<IActionResult> CreateBlogPost([FromBody] BlogPostRequest request)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized("User ID not found in token.");
    }

    var response = await blogpostService.CreateBlogPostAsync(userId, request);

    return ActionResultHelper.ToActionResult(response);
}
    /// <summary>
    /// Updates an existing blog post
    /// </summary>
    /// <param name="postId">Blog post ID</param>
    /// <param name="request">Blog post request object</param>
    /// <returns></returns>
    [HttpPut("{postId}")]
    public async Task<IActionResult> UpdateBlogPost(string postId, [FromBody] UpdateBlogpostRequest request)
    {
        var response = await blogpostService.UpdateBlogPostAsync(postId, request);
        
        return ActionResultHelper.ToActionResult(response);
    }

    /// <summary>
    /// Deletes a blog post
    /// </summary>
    /// <param name="postId">Blog post ID</param>
    /// <returns></returns>
    [HttpDelete("{postId}")]
    public async Task<IActionResult> DeleteBlogPost(string postId)
    {
        var response = await blogpostService.DeleteBlogPostAsync(postId);
        
        return ActionResultHelper.ToActionResult(response);
    }

    /// <summary>
    /// Gets a blog post by ID
    /// </summary>
    /// <param name="postId">Blog post ID</param>
    /// <returns></returns>
    [HttpGet("{postId}")]
    public async Task<IActionResult> GetBlogPostById(string postId)
    {
        var response = await blogpostService.GetBlogPostByIdAsync(postId);
        
        return ActionResultHelper.ToActionResult(response);
    }

    /// <summary>
    /// Gets all blog posts with pagination and filtering
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetAllBlogPosts([FromQuery] BaseFilter filter)
    {
        var response = await blogpostService.GetAllBlogPostsAsync(filter);
        
        return ActionResultHelper.ToActionResult(response);
    }
}