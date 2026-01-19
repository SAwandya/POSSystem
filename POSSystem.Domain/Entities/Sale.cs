namespace POSSystem.Domain.Entities;

public class Sale
{
    public int SaleId { get; set; }
    public int? CustomerId { get; set; }
    public int UserId { get; set; }
    public int SessionId { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.Now;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    // Navigation Properties
    public virtual Customer? Customer { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual DrawerSession DrawerSession { get; set; } = null!;
    public virtual ICollection<SalesItem> SalesItems { get; set; } = new List<SalesItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<SalesReturn> SalesReturns { get; set; } = new List<SalesReturn>();
}

public class SalesItem
{
    public int ItemId { get; set; }
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal QuantityReturned { get; set; } = 0;

    // Navigation Properties
    public virtual Sale Sale { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<SalesReturnItem> SalesReturnItems { get; set; } = new List<SalesReturnItem>();
}

public class Payment
{
    public int PaymentId { get; set; }
    public int SaleId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Now;

    // Navigation Properties
    public virtual Sale Sale { get; set; } = null!;
}

public enum PaymentStatus
{
    Pending,
    Partial,
    Paid
}

public enum PaymentMethod
{
    Cash,
    Card,
    Transfer,
    Credit
}
