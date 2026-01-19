namespace POSSystem.Application.DTOs;

public class SaleDto
{
    public int SaleId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string? CustomerName { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public List<SaleItemDto> Items { get; set; } = new();
}

public class SaleItemDto
{
    public int ItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateSaleDto
{
    public int? CustomerId { get; set; }
    public int UserId { get; set; }
    public int SessionId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public decimal AmountPaid { get; set; }
    public List<CreateSaleItemDto> Items { get; set; } = new();
}

public class CreateSaleItemDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class SaleResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? SaleId { get; set; }
    public decimal Change { get; set; }
}
