using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Ecom.OrderService.Core.Abstractions.Persistence
{
    public interface IRepository<T> where T : class
    {

        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        public Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
        Task<T> FindAsync(object Id);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<T> FirstOrDefaultAsNoTrackingAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> ToListAsync();
        Task<IEnumerable<T>> ToListAsync(Expression<Func<T, bool>> predicate);

        public IQueryable<T> ListIncludes(params Expression<Func<T, object>>[] includes);
        public IQueryable<T> GetAll(
           Expression<Func<T, bool>> predicate = null,
           Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
           Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool disableTracking = true, bool ignoreQueryFilters = false);
        public void Detached(T entity);
    }
}
