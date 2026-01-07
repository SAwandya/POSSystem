using POSSystem.Domain.Common;
using POSSystem.Domain.Enums;

namespace POSSystem.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int CategoryId { get; set; }
    public int CurrentStock { get; set; }
    public int MinStockLevel { get; set; } = 10;
    public int MaxStockLevel { get; set; } = 1000;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal TaxRate { get; set; } = 0.18m; // 18% default
    
    // Navigation Properties
    public Category Category { get; set; } = null!;
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<GRNItem> GRNItems { get; set; } = new List<GRNItem>();
    public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    
    // Computed Property
    public StockStatus StockStatus
    {
        get
        {
            if (CurrentStock <= 0) return StockStatus.OutOfStock;
            if (CurrentStock <= MinStockLevel) return StockStatus.LowStock;
            return StockStatus.InStock;
        }
    }
}
