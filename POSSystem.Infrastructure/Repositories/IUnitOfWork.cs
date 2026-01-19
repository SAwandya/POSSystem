namespace POSSystem.Infrastructure.Repositories;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ISalesRepository Sales { get; }
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
