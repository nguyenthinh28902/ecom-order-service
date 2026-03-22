namespace Ecom.OrderService.Core.Abstractions.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync(); // Trả về số dòng bị ảnh hưởng
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}