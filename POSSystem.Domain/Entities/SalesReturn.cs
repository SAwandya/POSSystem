namespace POSSystem.Domain.Entities;

public class SalesReturn
{
    public int ReturnId { get; set; }
    public int SaleId { get; set; }
    public int UserId { get; set; }
    public int SessionId { get; set; }
    public DateTime ReturnDate { get; set; } = DateTime.Now;
    public decimal TotalRefund { get; set; } = 0;
    public string? Reason { get; set; }

    // Navigation Properties
    public virtual Sale Sale { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual DrawerSession DrawerSession { get; set; } = null!;
    public virtual ICollection<SalesReturnItem> SalesReturnItems { get; set; } = new List<SalesReturnItem>();
}

public class SalesReturnItem
{
    public int ReturnItemId { get; set; }
    public int ReturnId { get; set; }
    public int SaleItemId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal RefundAmount { get; set; }
    public ReturnCondition ConditionStatus { get; set; } = ReturnCondition.Good;

    // Navigation Properties
    public virtual SalesReturn SalesReturn { get; set; } = null!;
    public virtual SalesItem SalesItem { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}

public enum ReturnCondition
{
    Good,
    Damaged
}
