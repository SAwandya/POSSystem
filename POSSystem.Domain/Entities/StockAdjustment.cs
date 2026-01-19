namespace POSSystem.Domain.Entities;

public class StockAdjustment
{
    public int AdjustmentId { get; set; }
    public int UserId { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime AdjustmentDate { get; set; } = DateTime.Now;
    public AdjustmentReason ReasonType { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<AdjustmentItem> AdjustmentItems { get; set; } = new List<AdjustmentItem>();
}

public class AdjustmentItem
{
    public int AdjItemId { get; set; }
    public int AdjustmentId { get; set; }
    public int ProductId { get; set; }
    public decimal? SystemQty { get; set; }
    public decimal? PhysicalQty { get; set; }
    public decimal? DifferenceQty { get; set; }

    // Navigation Properties
    public virtual StockAdjustment StockAdjustment { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}

public enum AdjustmentReason
{
    Damage,
    Theft,
    DataError,
    StockTake,
    Found,
    Other
}
