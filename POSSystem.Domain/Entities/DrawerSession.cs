namespace POSSystem.Domain.Entities;

public class DrawerSession
{
    public int SessionId { get; set; }
    public int UserId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
    public decimal OpeningCash { get; set; } = 0;
    public decimal ClosingCashSystem { get; set; } = 0;
    public decimal? ClosingCashActual { get; set; }
    public decimal? Variance { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Open;
    public string? Remarks { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public virtual ICollection<DrawerCashFlow> DrawerCashFlows { get; set; } = new List<DrawerCashFlow>();
    public virtual ICollection<SalesReturn> SalesReturns { get; set; } = new List<SalesReturn>();
}

public class DrawerCashFlow
{
    public int FlowId { get; set; }
    public int SessionId { get; set; }
    public decimal Amount { get; set; }
    public CashFlowType Type { get; set; }
    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    // Navigation Properties
    public virtual DrawerSession DrawerSession { get; set; } = null!;
}

public enum SessionStatus
{
    Open,
    Closed
}

public enum CashFlowType
{
    SafeDrop,
    PayOut,
    PayIn
}
