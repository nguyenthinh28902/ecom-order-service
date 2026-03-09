namespace Ecom.OrderService.Core.Abstractions.Persistence
{
    public interface IUnitOfWork
    {
        IRepository<T> Repository<T>() where T : class;
        Task SaveChangesAsync();
        Task CommitAsync();
        Task BeginTransactionAsync();
        void SaveChanges();
    }
}
