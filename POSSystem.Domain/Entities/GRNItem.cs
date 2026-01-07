using POSSystem.Domain.Common;

namespace POSSystem.Domain.Entities;

public class GRNItem : BaseEntity
{
    public int GRNId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Total { get; set; }
    
    // Navigation Properties
    public GRN GRN { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
