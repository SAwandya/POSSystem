namespace POSSystem.Application.DTOs;

public class ProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public decimal Quantity { get; set; }
    public int AlertQty { get; set; }
    public bool IsActive { get; set; }
    public bool IsLowStock { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public int? SubCatId { get; set; }
    public string UnitMeasure { get; set; } = "pcs";
    public int AlertQty { get; set; } = 10;
    public decimal SellingPrice { get; set; }
    public decimal InitialQuantity { get; set; } = 0;
}

public class UpdateProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal SellingPrice { get; set; }
    public int AlertQty { get; set; }
    public bool IsActive { get; set; }
}
