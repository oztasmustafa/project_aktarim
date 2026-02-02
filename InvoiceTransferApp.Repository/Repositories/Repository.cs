using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvoiceTransferApp.Core.Interfaces;
using InvoiceTransferApp.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTransferApp.Repository.Repositories
{
    /// <summary>
    /// Generic repository - tüm entity'ler için EF Core CRUD
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly InvoiceDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(InvoiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        public virtual IQueryable<T> Query() => _dbSet;

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public virtual void Update(T entity) => _dbSet.Update(entity);

        public virtual void Remove(T entity) => _dbSet.Remove(entity);

        public virtual void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);
    }
}
