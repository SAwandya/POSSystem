using POSSystem.Application.DTOs;
using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Repositories;

namespace POSSystem.Application.Services;

public class SalesService : ISalesService
{
    private readonly IUnitOfWork _unitOfWork;

    public SalesService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SaleResult> CreateSaleAsync(CreateSaleDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Validate stock availability
            foreach (var item in dto.Items)
            {
                var inventory = await _unitOfWork.Repository<Inventory>()
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                if (inventory == null || inventory.Quantity < item.Quantity)
                {
                    return new SaleResult
                    {
                        Success = false,
                        Message = $"Insufficient stock for product ID {item.ProductId}"
                    };
                }
            }

            // Create sale
            var sale = new Sale
            {
                CustomerId = dto.CustomerId,
                UserId = dto.UserId,
                SessionId = dto.SessionId,
                SaleDate = DateTime.Now,
                SubTotal = dto.SubTotal,
                TaxAmount = dto.TaxAmount,
                DiscountAmount = dto.DiscountAmount,
                GrandTotal = dto.GrandTotal,
                PaymentStatus = PaymentStatus.Paid
            };

            await _unitOfWork.Sales.AddAsync(sale);
            await _unitOfWork.SaveChangesAsync();

            // Add sale items and update inventory
            foreach (var itemDto in dto.Items)
            {
                var saleItem = new SalesItem
                {
                    SaleId = sale.SaleId,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = itemDto.Quantity * itemDto.UnitPrice
                };

                await _unitOfWork.Repository<SalesItem>().AddAsync(saleItem);

                // Update inventory
                var inventory = await _unitOfWork.Repository<Inventory>()
                    .FirstOrDefaultAsync(i => i.ProductId == itemDto.ProductId);

                if (inventory != null)
                {
                    inventory.Quantity -= itemDto.Quantity;
                    _unitOfWork.Repository<Inventory>().Update(inventory);
                }
            }

            // Add payment
            var payment = new Payment
            {
                SaleId = sale.SaleId,
                Amount = dto.AmountPaid,
                PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod, true),
                PaymentDate = DateTime.Now
            };

            await _unitOfWork.Repository<Payment>().AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            return new SaleResult
            {
                Success = true,
                Message = "Sale completed successfully",
                SaleId = sale.SaleId,
                Change = dto.AmountPaid - dto.GrandTotal
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return new SaleResult
            {
                Success = false,
                Message = $"Error processing sale: {ex.Message}"
            };
        }
    }

    public async Task<SaleDto?> GetSaleByIdAsync(int saleId)
    {
        var sale = await _unitOfWork.Sales.GetSaleWithDetailsAsync(saleId);
        return sale != null ? MapToDto(sale) : null;
    }

    public async Task<IEnumerable<SaleDto>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var sales = await _unitOfWork.Sales.GetSalesByDateRangeAsync(startDate, endDate);
        return sales.Select(MapToDto);
    }

    public async Task<IEnumerable<SaleDto>> GetTodaysSalesAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        return await GetSalesByDateRangeAsync(today, tomorrow);
    }

    public async Task<decimal> GetTodaysTotalAsync()
    {
        return await _unitOfWork.Sales.GetTotalSalesByDateAsync(DateTime.Today);
    }

    public async Task<int> GetTodaysSalesCountAsync()
    {
        return await _unitOfWork.Sales.GetSalesCountByDateAsync(DateTime.Today);
    }

    private static SaleDto MapToDto(Sale sale)
    {
        return new SaleDto
        {
            SaleId = sale.SaleId,
            InvoiceNumber = $"INV-{sale.SaleId:D6}",
            SaleDate = sale.SaleDate,
            CustomerName = sale.Customer?.Name ?? "Walk-in Customer",
            SubTotal = sale.SubTotal,
            TaxAmount = sale.TaxAmount,
            DiscountAmount = sale.DiscountAmount,
            GrandTotal = sale.GrandTotal,
            PaymentStatus = sale.PaymentStatus.ToString(),
            Items = sale.SalesItems.Select(si => new SaleItemDto
            {
                ItemId = si.ItemId,
                ProductId = si.ProductId,
                ProductName = si.Product.Name,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                TotalPrice = si.TotalPrice
            }).ToList()
        };
    }
}
