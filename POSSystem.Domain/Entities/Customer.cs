using POSSystem.Domain.Common;

namespace POSSystem.Domain.Entities;

public class Customer : BaseEntity
{
    public string CustomerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public decimal CreditLimit { get; set; } = 0;
    public decimal CurrentBalance { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public int TotalPurchases { get; set; } = 0;
    public decimal TotalSpent { get; set; } = 0;
    
    // Navigation Properties
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
