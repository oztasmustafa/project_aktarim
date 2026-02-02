using System;
using InvoiceTransferApp.Core.Entities;

namespace InvoiceTransferApp.Core.Interfaces
{
    /// <summary>
    /// Unit of Work pattern - transaction yönetimi için
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Invoice> Invoices { get; }
        // Diğer repository'ler eklenebilir
        
        int Complete();
    }
}
