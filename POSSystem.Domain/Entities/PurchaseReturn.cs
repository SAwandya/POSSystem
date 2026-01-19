namespace POSSystem.Domain.Entities;

public class PurchaseReturn
{
    public int PrId { get; set; }
    public int SupplierId { get; set; }
    public int UserId { get; set; }
    public DateTime ReturnDate { get; set; } = DateTime.Now;
    public PurchaseReturnStatus Status { get; set; } = PurchaseReturnStatus.Draft;
    public decimal TotalRefundAmount { get; set; } = 0;

    // Navigation Properties
    public virtual Supplier Supplier { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ICollection<PurchaseReturnItem> PurchaseReturnItems { get; set; } = new List<PurchaseReturnItem>();
}

public class PurchaseReturnItem
{
    public int PrItemId { get; set; }
    public int PrId { get; set; }
    public int GrnItemId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal RefundUnitCost { get; set; }
    public decimal TotalRefund { get; set; }
    public string? Reason { get; set; }

    // Navigation Properties
    public virtual PurchaseReturn PurchaseReturn { get; set; } = null!;
    public virtual GRNItem GRNItem { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}

public enum PurchaseReturnStatus
{
    Draft,
    Approved,
    Completed
}
