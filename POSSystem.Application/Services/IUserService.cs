using POSSystem.Application.DTOs;
using POSSystem.Domain.Entities;

namespace POSSystem.Application.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<(bool Success, string Message, int UserId)> CreateUserAsync(CreateUserDto dto);
    Task<(bool Success, string Message)> UpdateUserAsync(int userId, UpdateUserDto dto);
    Task<(bool Success, string Message)> ToggleUserStatusAsync(int userId);
    Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
    Task<IEnumerable<PermissionDto>> GetUserPermissionsAsync(int userId);
    Task<(bool Success, string Message)> UpdateUserPermissionsAsync(int userId, List<int> permissionIds);
    Task<bool> ValidateUserCredentialsAsync(string username, string password);
}

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string StatusText => IsActive ? "Active" : "Inactive";
    public List<PermissionDto> Permissions { get; set; } = new();
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public UserRole Role { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public class UpdateUserDto
{
    public string? FullName { get; set; }
    public UserRole? Role { get; set; }
    public string? NewPassword { get; set; }
}

public class PermissionDto
{
    public int PermissionId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ModuleGroup { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsGranted { get; set; }
}
