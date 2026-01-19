namespace POSSystem.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public UserRole Role { get; set; } = UserRole.Cashier;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation Properties
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public virtual ICollection<DrawerSession> DrawerSessions { get; set; } = new List<DrawerSession>();
    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public virtual ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}

public enum UserRole
{
    Admin,
    Manager,
    Cashier
}
