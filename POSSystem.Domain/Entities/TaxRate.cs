using POSSystem.Domain.Common;

namespace POSSystem.Domain.Entities;

public class TaxRate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
