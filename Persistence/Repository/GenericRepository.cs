using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Persistence.Data.Contexts;
using Shared;

namespace Persistence.Repository
{
    public class GenericRepository<T>(AppDbContext _context) : IGenericRepository<T> where T : class, IAduitable
    {
        public async Task<IReadOnlyList<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> result = _context.Set<T>().AsNoTracking();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    result = result.Include(include);
                }
            }
            return await result.ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> Predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> result = _context.Set<T>().AsNoTracking();
            if(includes != null)
            {
                foreach (var include in includes)
                {
                    result = result.Include(include);
                }
            }
            return await result.Where(Predicate).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> result = _context.Set<T>();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    result = result.Include(include);
                }
            }
            return await result.FirstOrDefaultAsync(e => EF.Property<int>(e, "id") == id);
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

       
        public async Task<PagedResult<T>> GetAsyncFilteredWithPaginate(Expression<Func<T, bool>> Predicate, Expression<Func<T, object>> orderBy, int? pageNumber = 1, int? pageSize = 10, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes.Any())
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            
            query = query.Where(Predicate);

            int page = pageNumber ?? 1;
            int size = pageSize ?? 10;

            if (page < 1) page = 1;
            int count = await query.CountAsync();
            
            query = query.OrderByDescending(orderBy)
                         .Skip((page - 1) * size)
                         .Take(size);

            return new PagedResult<T>(query.ToList(), count, page,size);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate) => await _context.Set<T>().CountAsync(predicate);
        

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => await _context.Set<T>().AnyAsync(predicate);

       
    }
}
