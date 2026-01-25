using Microsoft.EntityFrameworkCore;
using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Repositories;

namespace POSSystem.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Repository<User>().GetAllAsync();
        var userList = new List<UserDto>();

        foreach (var user in users)
        {
            var permissions = await GetUserPermissionsAsync(user.UserId);
            userList.Add(new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Permissions = permissions.ToList()
            });
        }

        return userList;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null) return null;

        var permissions = await GetUserPermissionsAsync(userId);
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Permissions = permissions.ToList()
        };
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var users = await _unitOfWork.Repository<User>().GetAllAsync();
        var user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        
        if (user == null) return null;

        var permissions = await GetUserPermissionsAsync(user.UserId);
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Permissions = permissions.ToList()
        };
    }

    public async Task<(bool Success, string Message, int UserId)> CreateUserAsync(CreateUserDto dto)
    {
        try
        {
            // Check if username already exists
            var existingUser = await GetUserByUsernameAsync(dto.Username);
            if (existingUser != null)
            {
                return (false, "Username already exists", 0);
            }

            // Hash password (simple hash for demo - use proper hashing in production)
            var passwordHash = HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = passwordHash,
                FullName = dto.FullName,
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Add permissions
            if (dto.PermissionIds.Any())
            {
                foreach (var permissionId in dto.PermissionIds)
                {
                    var userPermission = new UserPermission
                    {
                        UserId = user.UserId,
                        PermissionId = permissionId
                    };
                    await _unitOfWork.Repository<UserPermission>().AddAsync(userPermission);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return (true, "User created successfully", user.UserId);
        }
        catch (Exception ex)
        {
            return (false, $"Error creating user: {ex.Message}", 0);
        }
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(int userId, UpdateUserDto dto)
    {
        try
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.FullName = dto.FullName;
            }

            if (dto.Role.HasValue)
            {
                user.Role = dto.Role.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                user.PasswordHash = HashPassword(dto.NewPassword);
            }

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, "User updated successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating user: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> ToggleUserStatusAsync(int userId)
    {
        try
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            user.IsActive = !user.IsActive;
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            var status = user.IsActive ? "activated" : "deactivated";
            return (true, $"User {status} successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error toggling user status: {ex.Message}");
        }
    }

    public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
    {
        var permissions = await _unitOfWork.Repository<Permission>().GetAllAsync();
        return permissions.Select(p => new PermissionDto
        {
            PermissionId = p.PermissionId,
            Slug = p.Slug,
            Name = p.Name,
            ModuleGroup = p.ModuleGroup,
            Description = p.Description,
            IsGranted = false
        });
    }

    public async Task<IEnumerable<PermissionDto>> GetUserPermissionsAsync(int userId)
    {
        var allPermissions = await GetAllPermissionsAsync();
        var userPermissions = await _unitOfWork.Repository<UserPermission>()
            .FindAsync(up => up.UserId == userId);

        var userPermissionIds = userPermissions.Select(up => up.PermissionId).ToList();

        return allPermissions.Select(p => new PermissionDto
        {
            PermissionId = p.PermissionId,
            Slug = p.Slug,
            Name = p.Name,
            ModuleGroup = p.ModuleGroup,
            Description = p.Description,
            IsGranted = userPermissionIds.Contains(p.PermissionId)
        });
    }

    public async Task<(bool Success, string Message)> UpdateUserPermissionsAsync(int userId, List<int> permissionIds)
    {
        try
        {
            // Remove existing permissions
            var existingPermissions = await _unitOfWork.Repository<UserPermission>()
                .FindAsync(up => up.UserId == userId);

            foreach (var permission in existingPermissions)
            {
                _unitOfWork.Repository<UserPermission>().Remove(permission);
            }
            await _unitOfWork.SaveChangesAsync();

            // Add new permissions
            foreach (var permissionId in permissionIds)
            {
                var userPermission = new UserPermission
                {
                    UserId = userId,
                    PermissionId = permissionId
                };
                await _unitOfWork.Repository<UserPermission>().AddAsync(userPermission);
            }
            await _unitOfWork.SaveChangesAsync();

            return (true, "Permissions updated successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating permissions: {ex.Message}");
        }
    }

    public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
    {
        var user = await GetUserByUsernameAsync(username);
        if (user == null || !user.IsActive) return false;

        var dbUser = await _unitOfWork.Repository<User>().GetByIdAsync(user.UserId);
        if (dbUser == null) return false;

        return dbUser.PasswordHash == HashPassword(password);
    }

    private string HashPassword(string password)
    {
        // Simple hash for demo - in production use BCrypt, PBKDF2, or Argon2
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
