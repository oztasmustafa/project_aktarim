using InvoiceTransferApp.Core.Entities;
using InvoiceTransferApp.Core.Interfaces;
using InvoiceTransferApp.Repository.Context;
using InvoiceTransferApp.Repository.Repositories;

namespace InvoiceTransferApp.Repository.UnitOfWorks
{
    /// <summary>
    /// Unit of Work implementation - transaction yönetimi
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly InvoiceDbContext _context;
        private InvoiceRepository _invoiceRepository;

        public UnitOfWork(InvoiceDbContext context)
        {
            _context = context;
        }

        public IRepository<Invoice> Invoices
        {
            get
            {
                if (_invoiceRepository == null)
                    _invoiceRepository = new InvoiceRepository(_context);
                return _invoiceRepository;
            }
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
