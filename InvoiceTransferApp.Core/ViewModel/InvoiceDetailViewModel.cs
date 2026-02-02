using System;
using System.Collections.Generic;

namespace InvoiceTransferApp.Core.ViewModel
{
    /// <summary>
    /// Fatura detay formu görünüm modeli
    /// </summary>
    public class InvoiceDetailViewModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public bool IsTransferredToNetsis { get; set; }
        public DateTime? TransferDate { get; set; }
        public string NetsisReferenceNumber { get; set; }
        public List<InvoiceItemDetailViewModel> Items { get; set; } = new List<InvoiceItemDetailViewModel>();
    }

    /// <summary>
    /// Fatura kalem detay görünüm modeli
    /// </summary>
    public class InvoiceItemDetailViewModel
    {
        public int Id { get; set; }
        public int ItemNo { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
