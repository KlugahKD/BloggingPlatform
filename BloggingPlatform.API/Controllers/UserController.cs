using BloggingPlatform.API.Helper;
using BloggingPlatform.Business.Common;
using BloggingPlatform.Business.Models.BaseModels;
using BloggingPlatform.Business.Models.Requests;
using BloggingPlatform.Business.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BloggingPlatform.API.Controllers;

/// <summary>
/// User Endpoint for CRUD operations
/// </summary>
/// <param name="userService"></param>
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Adds a new user to the database
    /// </summary>
    /// <param name="request">Add user request object</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody] AddUserRequest request)
    {
        var response = await userService.RegisterUserAsync(request, User);
        
        return ActionResultHelper.ToActionResult(response);
    }

    /// <summary>
    /// Updates a user in the database
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Update user request object</param>
    /// <returns></returns>
    [Authorize(Roles = Constants.Roles.Admin)]
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
    {
        var response = await userService.UpdateUserAsync(userId, request);
        
        return ActionResultHelper.ToActionResult(response);
    }

    /// <summary>
    /// Deletes a user from the database
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns></returns>
    [Authorize(Roles = Constants.Roles.Admin)]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var response = await userService.DeleteUserAsync(userId);
        
        return ActionResultHelper.ToActionResult(response);
    }

    /// <summary>
    /// Gets a single user from the database
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns></returns>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        var response = await userService.GetUserByIdAsync(userId);
        
        return ActionResultHelper.ToActionResult(response);
    }
    
    /// <summary>
    /// Logs in a user
    /// </summary>
    /// <param name="request">Login request object</param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await userService.LoginAsync(request);
        
        return ActionResultHelper.ToActionResult(response);
    }

    /// <summary>
    /// Gets all users with pagination and filtering
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] BaseFilter filter)
    {
        var response = await userService.GetAllUsersAsync(filter);
        
        return ActionResultHelper.ToActionResult(response);
    }
}