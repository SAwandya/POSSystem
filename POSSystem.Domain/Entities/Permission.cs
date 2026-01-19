namespace POSSystem.Domain.Entities;

public class Permission
{
    public int PermissionId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ModuleGroup { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation Properties
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}

public class UserPermission
{
    public int UserId { get; set; }
    public int PermissionId { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}

public class ActivityLog
{
    public int LogId { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    // Navigation Properties
    public virtual User? User { get; set; }
}
