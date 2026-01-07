using POSSystem.Domain.Common;

namespace POSSystem.Domain.Entities;

public class SaleItem : BaseEntity
{
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    
    // Navigation Properties
    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
