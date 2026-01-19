namespace POSSystem.Domain.Entities;

public class Inventory
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; } = 0;
    public decimal AverageCost { get; set; } = 0;
    public decimal SellingPrice { get; set; } = 0;
    public DateTime? ExpiryDate { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
}
