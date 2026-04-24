using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Ecom.OrderService.Core.Abstractions.Persistence
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> Entities { get; }
        IQueryable<T> EntitiesNoTracking { get; }
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        public void Detached(T entity);
    }
}
