using System;
using System.Collections.Generic;

namespace InvoiceTransferApp.Core.DTO
{
    /// <summary>
    /// Fatura veri aktarım nesnesi (katmanlar arası)
    /// </summary>
    public class InvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public string CompanyCode { get; set; }
        public string InvoiceType { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsTransferredToNetsis { get; set; }
        public DateTime? TransferDate { get; set; }
        public string NetsisReferenceNumber { get; set; }
        public string Notes { get; set; }
        public List<InvoiceItemDto> Items { get; set; } = new List<InvoiceItemDto>();

        // Netsis grid kolonları
        public string CariKod2 { get; set; }
        public string Kod1 { get; set; }
        public int? OdemeGunu { get; set; }
        public string PlaKodu { get; set; }
        public string PlasiyerAciklama { get; set; }
        public string GibFatirsNo { get; set; }
        public bool? KdvDahilmi { get; set; }
        public int? FatKalemAdedi { get; set; }
        public decimal Bruttutar { get; set; }
        public decimal SatIskt { get; set; }
        public decimal MfazIskt { get; set; }
        public decimal GenIsk1T { get; set; }
        public decimal GenIsk1O { get; set; }
        public decimal GenIsk2T { get; set; }
        public decimal GenIsk2O { get; set; }
        public decimal GenIsk3T { get; set; }
        public decimal GenIsk3O { get; set; }
        public decimal GenelToplam { get; set; }
        public string FatAltm1 { get; set; }
        public string FatAltm2 { get; set; }
        public string FatAltm3 { get; set; }
        public string Acik1 { get; set; }
        public string Acik2 { get; set; }
        public string Acik3 { get; set; }
        public string Acik4 { get; set; }
        public string Acik5 { get; set; }
        public string Acik6 { get; set; }
        public string Acik7 { get; set; }
        public string Acik8 { get; set; }
        public string Acik9 { get; set; }
        public string Acik10 { get; set; }
        public string Acik11 { get; set; }
        public string Acik12 { get; set; }
        public string Acik13 { get; set; }
        public string Acik14 { get; set; }
        public string Acik15 { get; set; }
        public string Acik16 { get; set; }
    }
}
