using POSSystem.Application.DTOs;

namespace POSSystem.Application.Services;

public interface ISalesService
{
    Task<SaleResult> CreateSaleAsync(CreateSaleDto dto);
    Task<SaleDto?> GetSaleByIdAsync(int saleId);
    Task<IEnumerable<SaleDto>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<SaleDto>> GetTodaysSalesAsync();
    Task<decimal> GetTodaysTotalAsync();
    Task<int> GetTodaysSalesCountAsync();
}
