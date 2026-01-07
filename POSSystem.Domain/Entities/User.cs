using POSSystem.Domain.Common;
using POSSystem.Domain.Enums;

namespace POSSystem.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation Properties
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
