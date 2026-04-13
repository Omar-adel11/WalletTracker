using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Persistence.Data.Contexts;

namespace Persistence.Repository
{
    public class UnitOfWork(AppDbContext _context) : IUnitOfWork
    {
        private ConcurrentDictionary<string,object> _repositories = new ConcurrentDictionary<string,object>();
        public IGenericRepository<T> Repository<T>() where T : class,IAduitable
        {
            return (IGenericRepository<T>)_repositories.GetOrAdd(typeof(T).Name,_ =>  new GenericRepository<T>(_context));
        }
        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public async ValueTask DisposeAsync() => await _context.DisposeAsync();

    }
}
