using POSSystem.Domain.Common;

namespace POSSystem.Domain.Entities;

public class Supplier : BaseEntity
{
    public string SupplierCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? ContactPerson { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public ICollection<GRN> GRNs { get; set; } = new List<GRN>();
}
