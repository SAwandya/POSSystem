namespace POSSystem.Domain.Entities;

public class GRN
{
    public int GrnId { get; set; }
    public int SupplierId { get; set; }
    public int UserId { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime ReceivedDate { get; set; } = DateTime.Now;
    public decimal? TotalAmount { get; set; }

    // Navigation Properties
    public virtual Supplier Supplier { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ICollection<GRNItem> GRNItems { get; set; } = new List<GRNItem>();
}

public class GRNItem
{
    public int GrnItemId { get; set; }
    public int GrnId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal QuantityReturned { get; set; } = 0;

    // Navigation Properties
    public virtual GRN GRN { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<PurchaseReturnItem> PurchaseReturnItems { get; set; } = new List<PurchaseReturnItem>();
}
