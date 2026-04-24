using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Contracts
{
    public interface IGenericRepository<T> where T : class , IAduitable
    {
        Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);
        Task<IReadOnlyList<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T,bool>> Predicate, params Expression<Func<T, object>>[] includes);
        Task<IReadOnlyList<T>> GetAsyncFilteredWithPaginate(Expression<Func<T,bool>> Predicate, Expression<Func<T, object>> orderBy, int? pageNumber = 1, int? pageSize = 10, params Expression<Func<T, object>>[] includes);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
