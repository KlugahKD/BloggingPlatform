using BloggingPlatform.Business.Common;
using BloggingPlatform.Business.Models.BaseModels;
using BloggingPlatform.Business.Models.Requests;
using BloggingPlatform.Business.Models.Responses;
using BloggingPlatform.Data.Entities;

namespace BloggingPlatform.Business.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResponse<UserResponse>> RegisterUserAsync(AddUserRequest userRequest);
    Task<ServiceResponse<string>> LoginAsync (LoginRequest loginRequest);
    Task<ServiceResponse<UserResponse>> GetUserByIdAsync(string userId);
    Task<ServiceResponse<UserResponse>> UpdateUserAsync(string userId, UpdateUserRequest userRequest);
    Task<ServiceResponse<UserResponse>> DeleteUserAsync(string userId);
    Task<ServiceResponse<PagedResult<UserResponse>>> GetAllUsersAsync(BaseFilter filter);
}