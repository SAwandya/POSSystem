using POSSystem.Domain.Common;
using POSSystem.Domain.Enums;

namespace POSSystem.Domain.Entities;

public class InventoryTransaction : BaseEntity
{
    public int ProductId { get; set; }
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    
    // Navigation Properties
    public Product Product { get; set; } = null!;
}
