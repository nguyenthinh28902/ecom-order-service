using Ecom.OrderService.Core.Abstractions.Persistence;
using Ecom.OrderService.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Ecom.OrderService.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private EcomOrderDbContext _context;
        public Repository(EcomOrderDbContext _context)
        {
            this._context = _context;
        }

        /// <summary>
        /// add 1 item
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        /// <summary>
        /// add nhiều item
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
        }

        /// <summary>
        /// Xóa 1 item
        /// </summary>
        /// <param name="entity"></param>
        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
        /// <summary>
        /// xóa nhiều item
        /// </summary>
        /// <param name="entities"></param>
        public void RemoveRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        /// <summary>
        /// cập nhật 1 item
        /// </summary>
        /// <param name="entity"></param>
        public void Update(T entity)
        {
            _context.Update(entity);
        }

        /// <summary>
        /// cập nhật nhiều item
        /// </summary>
        /// <param name="entities"></param>
        public void UpdateRange(IEnumerable<T> entities)
        {
            _context.UpdateRange(entities);
        }

        /// <summary>
        /// get 1 item theo id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<T> FindAsync(object Id)
        {
            var entity = await _context.Set<T>().FindAsync(Id);
            if (entity == null) return null;
            return entity;
        }

        /// <summary>
        /// get 1 item theo điều kiện lambda 
        /// </summary>
        /// <param name="predicate">lambda</param>
        /// <returns></returns>
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            var entity = await _context.Set<T>().FirstOrDefaultAsync(predicate);
            if (entity == null) return null;
            return entity;
        }

        /// <summary>
        /// get 1 item theo điều kiện lambda  kèm AsNoTracking
        /// </summary>
        /// <param name="predicate">lambda</param>
        /// <returns></returns>
        public async Task<T> FirstOrDefaultAsNoTrackingAsync(Expression<Func<T, bool>> predicate)
        {
            var entity = await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(predicate);
            if (entity == null) return null;
            return entity;
        }
        /// <summary>
        /// Get all danh sach 
        /// trả về là kiểu dữ liệu danh sách
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<T>> ToListAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }
        /// <summary>
        /// đếm số lượng theo diều kiện lambda
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            return await _context.Set<T>().AsNoTracking().Where(predicate).CountAsync();
        }
        /// <summary>
        /// get danh sách có điều kiện lambda
        /// AsNoTracking
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> ToListAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AsNoTracking().Where(predicate).ToListAsync();
        }
        /// <summary>
        /// get danh sách có includes
        /// </summary>
        /// <param name="includes"></param>
        /// <returns></returns>
        public IQueryable<T> ListIncludes(params Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().AsQueryable().AsNoTracking();
            return includes.Aggregate(query, (q, w) => q.Include(w));
        }
        /// <summary>
        /// get danh sách có điều kiện lambda, có includes, có orderby
        /// </summary>
        /// <param name="predicate">điều kiện</param>
        /// <param name="orderBy"></param>
        /// <param name="include"></param>
        /// <param name="disableTracking"></param>
        /// <param name="ignoreQueryFilters"></param>
        /// <returns></returns>
        public IQueryable<T> GetAll(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool disableTracking = true, bool ignoreQueryFilters = false)
        {
            var query = _context.Set<T>().AsQueryable();

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }          

            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            if (include != null)
            {
                query = include(query);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (orderBy != null)
            {
                return orderBy(query);
            }
            else
            {
                return query;
            }
        }

        public void Detached(T entity)
        {

            _context.Entry(entity).State = EntityState.Detached;
        }
    }
}
