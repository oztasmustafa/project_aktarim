using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvoiceTransferApp.Core.DTO;

namespace InvoiceTransferApp.Core.Interfaces
{
    /// <summary>
    /// Invoice business logic interface
    /// </summary>
    public interface IInvoiceService
    {
        // Query operations - DTO döner
        List<InvoiceDto> GetSalesInvoices(string sourceCompany, DateTime startDate, DateTime endDate);
        InvoiceDto GetInvoiceById(int id);
        
        // Command operations
        string TransferInvoice(int invoiceId, string targetCompanyCode);
        Task<string> TransferInvoiceAsync(int invoiceId, string targetCompanyCode);
        bool ValidateInvoice(InvoiceDto invoice);
    }
}
