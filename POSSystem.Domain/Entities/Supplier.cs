namespace POSSystem.Domain.Entities;

public class Supplier
{
    public int SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    // Navigation Properties
    public virtual ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
    public virtual ICollection<GRN> GRNs { get; set; } = new List<GRN>();
    public virtual ICollection<PurchaseReturn> PurchaseReturns { get; set; } = new List<PurchaseReturn>();
}

public class ProductSupplier
{
    public int ProductId { get; set; }
    public int SupplierId { get; set; }
    public string? Note { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual Supplier Supplier { get; set; } = null!;
}
