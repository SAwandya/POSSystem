using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystem.Domain.Entities;

public class Product
{
    public int ProductId { get; set; }
    public int? SubCatId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public string UnitMeasure { get; set; } = "pcs";

    [Column("unit_price")]
    public decimal UnitPrice { get; set; } = 0;

    public int AlertQty { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation Properties
    public virtual SubCategory? SubCategory { get; set; }
    public virtual Inventory? Inventory { get; set; }
    public virtual ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
    public virtual ICollection<SalesItem> SalesItems { get; set; } = new List<SalesItem>();
    public virtual ICollection<GRNItem> GRNItems { get; set; } = new List<GRNItem>();
    public virtual ICollection<AdjustmentItem> AdjustmentItems { get; set; } = new List<AdjustmentItem>();
}
