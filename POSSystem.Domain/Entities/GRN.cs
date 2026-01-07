using POSSystem.Domain.Common;

namespace POSSystem.Domain.Entities;

public class GRN : BaseEntity
{
    public string GRNNumber { get; set; } = string.Empty;
    public DateTime GRNDate { get; set; } = DateTime.Now;
    public int SupplierId { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; } = false;
    
    // Navigation Properties
    public Supplier Supplier { get; set; } = null!;
    public ICollection<GRNItem> GRNItems { get; set; } = new List<GRNItem>();
}
