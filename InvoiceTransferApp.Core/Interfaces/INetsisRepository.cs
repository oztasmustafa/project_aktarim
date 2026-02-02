using System;
using System.Collections.Generic;
using InvoiceTransferApp.Core.Entities;

namespace InvoiceTransferApp.Core.Interfaces
{
    /// <summary>
    /// Netsis entegrasyonu için özel repository interface
    /// </summary>
    public interface INetsisRepository
    {
        List<Invoice> GetSalesInvoices(string sourceCompany, DateTime startDate, DateTime endDate);
        Invoice GetInvoiceById(int id);
        string SaveInvoice(Fatura fatura, string targetCompanyCode);
    }
}
