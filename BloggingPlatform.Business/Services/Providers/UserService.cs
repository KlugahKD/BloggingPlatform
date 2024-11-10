using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BloggingPlatform.Business.Services.Providers;

public class UserService(
    IGenericRepository<User> userRepository,
    ILogger<UserService> logger,
    IPasswordHasher passwordHasher,
    IConfiguration configuration) : IUserService
{
    public async Task<ServiceResponse<UserResponse>> RegisterUserAsync(AddUserRequest userRequest,  ClaimsPrincipal userPrincipal)
    {
        try
        {
            logger.LogInformation("Registering user");

            var isValidPhoneNumber = PhoneNumber.CorrectPhoneNumber(userRequest.PhoneNumber);
            if (string.IsNullOrWhiteSpace(isValidPhoneNumber))
            {
                logger.LogDebug("Invalid phone number.\r\nUserRequest: {Request}", userRequest.PhoneNumber);

                return Response.BadRequestResponse<UserResponse>("Invalid phone number");
            }
            
            if (userRequest.Password.Length < 6)
            {
                logger.LogDebug("Password is too short.\r\nUserRequest: {Request}", userRequest.PhoneNumber);

                return Response.BadRequestResponse<UserResponse>("Password is too short");
            }

            var userAlreadyExists = await userRepository.ExistsAsync(u =>
                u.PhoneNumber == isValidPhoneNumber || u.Email == userRequest.Email);
            if (userAlreadyExists)
            {
                logger.LogDebug("User with PhoneNumber already exists.\r\nUserRequest: {Request}",
                    userRequest.PhoneNumber);

                return Response.BadRequestResponse<UserResponse>("User already exists");
            }
            
            var createdBy = userPrincipal.Identity?.Name ?? Constants.CreatedBy.System;


            var user = new User()
            {
                Id = Guid.NewGuid().ToString("N"),
                FullName = userRequest.FullName,
                Email = userRequest.Email,
                PhoneNumber = isValidPhoneNumber,
                Password = passwordHasher.HashPassword(userRequest.Password),
                CreatedAt = DateTime.Now,
                IsActive = true,
                Role = Constants.Roles.User,
                CreatedBy = createdBy
            };

            var isSaved = await userRepository.AddAsync(user);

            if (!isSaved)
            {
                logger.LogError("Error occurred while saving user.\r\nUserRequest: {Request}", userRequest);

                return Response.FailedDependencyResponse<UserResponse>("Error occurred while saving user");
            }

            logger.LogInformation("User registered");

            return Response.OkResponse(user.Adapt<UserResponse>());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error registering user");

            return Response.InternalServerErrorResponse<UserResponse>(Constants.Response.InternalServerError);
        }
    }

    public async Task<ServiceResponse<string>> LoginAsync(LoginRequest loginRequest)
    {
        try
        {
            logger.LogInformation("Logging in user");

            var isValidPhoneNumber = PhoneNumber.CorrectPhoneNumber(loginRequest.PhoneNumber);
            if (string.IsNullOrWhiteSpace(isValidPhoneNumber))
            {
                logger.LogDebug("Invalid phone number.\r\nUserRequest: {Request}", loginRequest.PhoneNumber);

                return Response.BadRequestResponse<string>("Invalid phone number");
            }

            var user = await userRepository.FindAsync(u =>
                u.PhoneNumber == isValidPhoneNumber && u.IsActive);

            if (user is null || !user.IsActive)
            {
                logger.LogDebug("User with PhoneNumber does not exist.\r\nUserRequest: {Request}",
                    loginRequest.PhoneNumber);

                return Response.BadRequestResponse<string>("Incorrect Login Credentials");
            }

            var verifyPassword = passwordHasher.VerifyPassword(user.Password, loginRequest.Password);

            if (!verifyPassword)
            {
                logger.LogDebug("User with PhoneNumber password is incorrect.\r\nUserRequest: {Request}",
                    loginRequest.PhoneNumber);

                return Response.BadRequestResponse<string>("Incorrect Login Credentials");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = configuration["JwtSettings:Issuer"],
                Audience = configuration["JwtSettings:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            logger.LogInformation("Token Created Successfully");
            return Response.OkResponse(tokenString);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error logging in user");

            return Response.InternalServerErrorResponse<string>(Constants.Response.InternalServerError);
        }
    }

    public async Task<ServiceResponse<UserResponse>> GetUserByIdAsync(string userId)
    {
        try
        {
            logger.LogInformation("Getting user by ID {UserId}", userId);

            var user = await userRepository.FindAsync(x => x.Id == userId);

            if (user is null || !user.IsActive)
            {
                logger.LogDebug("User does not exist.\r\nTransactionRequest: {Request}", userId);

                return Response.NotFoundResponse<UserResponse>("User does not exist");
            }

            logger.LogInformation("User with Id: {Id} found", userId);

            return Response.OkResponse(user.Adapt<UserResponse>());

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving user with ID {UserId}", userId);

            return Response.InternalServerErrorResponse<UserResponse>(Constants.Response.InternalServerError);
        }
    }

    public async Task<ServiceResponse<UserResponse>> UpdateUserAsync(string userId, UpdateUserRequest userRequest)
    {
        try
        {
            var user = await userRepository.FindAsync(u => u.Id == userId && u.IsActive);

            if (user == null || !user.IsActive)
            {
                logger.LogDebug("User with ID {UserId} not found.", userId);
                return Response.NotFoundResponse<UserResponse>("User not found");
            }

            user.FullName = userRequest.FullName ?? user.FullName;
            user.Email = userRequest.Email ?? user.Email;
            user.PhoneNumber = userRequest.PhoneNumber ?? user.PhoneNumber;
            user.UpdatedAt = DateTime.Now;
            user.Role = userRequest.Role ?? user.Role;

            var isUpdated = await userRepository.UpdateAsync(user);

            if (!isUpdated)
            {
                logger.LogError("Error occurred while updating user.\r\nUserRequest: {Request}", userRequest);

                return Response.FailedDependencyResponse<UserResponse>("Error occurred while updating user");
            }

            logger.LogInformation("User with ID {UserId} updated successfully.", userId);

            return Response.OkResponse(user.Adapt<UserResponse>());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating user with ID {UserId}", userId);

            return Response.InternalServerErrorResponse<UserResponse>(Constants.Response.InternalServerError);
        }

    }

    public async Task<ServiceResponse<UserResponse>> DeleteUserAsync(string userId)
    {
        try
        {
            var user = await userRepository.FindAsync(u => u.Id == userId && u.IsActive);

            if (user == null || !user.IsActive)
            {
                logger.LogDebug("User with ID {UserId} not found.", userId);

                return Response.NotFoundResponse<UserResponse>("User not found");
            }

            var isUpdated = await userRepository.SoftDeleteAsync(userId);

            if (!isUpdated)
            {
                logger.LogError("Error occurred while deleting user.\r\nUserId: {UserId}", userId);

                return Response.FailedDependencyResponse<UserResponse>("Error occurred while deleting user");
            }

            logger.LogInformation("User with ID {UserId} deleted successfully.", userId);

            return Response.OkResponse(user.Adapt<UserResponse>());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting user with ID {UserId}", userId);
            return Response.InternalServerErrorResponse<UserResponse>(Constants.Response.InternalServerError);
        }
    }
    
    public async Task<ServiceResponse<PagedResult<UserResponse>>> GetAllUsersAsync(BaseFilter filter)
    {
        try
        {
            logger.LogInformation("Getting all users with search {Search}", filter.Search);

            var query = userRepository.AsQueryable().Where(u => u.IsActive);

            if (filter.CreatedAt.HasValue)
            {
                query = query.Where(u => u.CreatedAt == filter.CreatedAt.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(u => u.FullName.Contains(filter.Search) || u.Email.Contains(filter.Search));
            }

            var totalCount = await query.CountAsync();

            logger.LogInformation("Total users found {TotalCount}", totalCount);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    Role = u.Role,
                    IsDeleted = u.IsDeleted,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync();

            var response = new PagedResult<UserResponse>
            {
                Page = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount,
                Payload = users,
            };

            return Response.OkResponse(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get all users. {Ex}", e.Message);

            return Response.InternalServerErrorResponse<PagedResult<UserResponse>>(Constants.Response.InternalServerError);
        }
    }
}
