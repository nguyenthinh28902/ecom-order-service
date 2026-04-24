using Ecom.OrderService.Core.Abstractions.Persistence;
using Ecom.OrderService.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Ecom.OrderService.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly EcomOrderDbContext _context;

        public Repository(EcomOrderDbContext context)
        {
            _context = context;
        }

        public IQueryable<T> Entities => _context.Set<T>();
        public IQueryable<T> EntitiesNoTracking => _context.Set<T>().AsNoTracking();
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
        /// đếm số lượng theo diều kiện lambda
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            var query = _context.Set<T>().AsQueryable();

            // Chỉ thực hiện Where nếu predicate có giá trị
            if (predicate != null)
            {
                query = query.Where(predicate).AsNoTracking();
            }

            return await query.CountAsync();
        }


        public void Detached(T entity)
        {

            _context.Entry(entity).State = EntityState.Detached;
        }
    }
}
