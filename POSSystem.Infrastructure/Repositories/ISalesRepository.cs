using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Repositories;

public interface ISalesRepository : IRepository<Sale>
{
    Task<Sale?> GetSaleWithDetailsAsync(int saleId);
    Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Sale>> GetSalesBySessionAsync(int sessionId);
    Task<IEnumerable<Sale>> GetSalesByCustomerAsync(int customerId);
    Task<decimal> GetTotalSalesByDateAsync(DateTime date);
    Task<int> GetSalesCountByDateAsync(DateTime date);
}
