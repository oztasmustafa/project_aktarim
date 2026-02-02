using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceTransferApp.Core.Interfaces
{
    /// <summary>
    /// Generic repository interface - tüm entity'ler için temel CRUD operasyonları
    /// </summary>
    public interface IRepository<T> where T : class
    {
        IQueryable<T> Query();
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        Task AddRangeAsync(IEnumerable<T> entities);
    }
}
