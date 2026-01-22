using Microsoft.EntityFrameworkCore;
using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Data;

namespace POSSystem.Infrastructure.Repositories;

public class SalesRepository : Repository<Sale>, ISalesRepository
{
    public SalesRepository(POSDbContext context) : base(context)
    {
    }

    public async Task<Sale?> GetSaleWithDetailsAsync(int saleId)
    {
        return await _dbSet
            .Include(s => s.SalesItems)
                .ThenInclude(si => si.Product)
            .Include(s => s.Customer)
            .Include(s => s.User)
            .Include(s => s.Payments)
            .AsSplitQuery() // Prevents cartesian explosion
            .FirstOrDefaultAsync(s => s.SaleId == saleId);
    }

    public async Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(s => s.SalesItems)
                .ThenInclude(si => si.Product)
            .Include(s => s.Customer)
            .Include(s => s.Payments)
            .AsSplitQuery() // Prevents cartesian explosion
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesBySessionAsync(int sessionId)
    {
        return await _dbSet
            .Include(s => s.SalesItems)
                .ThenInclude(si => si.Product)
            .Include(s => s.Customer)
            .Include(s => s.Payments)
            .Where(s => s.SessionId == sessionId)
            .OrderBy(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesByCustomerAsync(int customerId)
    {
        return await _dbSet
            .Include(s => s.SalesItems)
                .ThenInclude(si => si.Product)
            .Include(s => s.Payments)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalSalesByDateAsync(DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        return await _dbSet
            .Where(s => s.SaleDate >= startDate && s.SaleDate < endDate && s.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(s => s.GrandTotal);
    }

    public async Task<int> GetSalesCountByDateAsync(DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        return await _dbSet
            .Where(s => s.SaleDate >= startDate && s.SaleDate < endDate)
            .CountAsync();
    }
}
