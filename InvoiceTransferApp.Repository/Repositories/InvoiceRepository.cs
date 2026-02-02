using InvoiceTransferApp.Core.Entities;
using InvoiceTransferApp.Core.Interfaces;
using InvoiceTransferApp.Repository.Context;

namespace InvoiceTransferApp.Repository.Repositories
{
    /// <summary>
    /// Invoice repository - generic Repository&lt;Invoice&gt; kullanır
    /// </summary>
    public class InvoiceRepository : Repository<Invoice>, IRepository<Invoice>
    {
        public InvoiceRepository(InvoiceDbContext context) : base(context)
        {
        }
    }
}
