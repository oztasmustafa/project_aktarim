using System;
using System.ComponentModel;

namespace InvoiceTransferApp.Core.ViewModel
{
    /// <summary>
    /// Fatura listesi grid görünüm modeli
    /// </summary>
    public class InvoiceListViewModel
    {
        [DisplayName("ID")]
        public int Id { get; set; }

        [DisplayName("Fatura No")]
        public string InvoiceNumber { get; set; }

        [DisplayName("Fatura Tarihi")]
        public DateTime InvoiceDate { get; set; }

        [DisplayName("Cari Kod")]
        public string CustomerCode { get; set; }

        [DisplayName("Cari Adı")]
        public string CustomerName { get; set; }

        [DisplayName("Toplam Tutar")]
        public decimal TotalAmount { get; set; }

        [DisplayName("Döviz")]
        public string Currency { get; set; }

        [DisplayName("Açıklama")]
        public string Description { get; set; }

        [DisplayName("Durum")]
        public string Status { get; set; }

        [DisplayName("Netsis'e Aktarıldı")]
        public bool IsTransferredToNetsis { get; set; }

        [DisplayName("Aktarım Tarihi")]
        public DateTime? TransferDate { get; set; }

        [DisplayName("Netsis Referans No")]
        public string NetsisReferenceNumber { get; set; }

        // Netsis grid kolonları (görüntülenecek alanlar)
        [DisplayName("Şube Kodu")]
        public string CompanyCode { get; set; }
        [DisplayName("Cari Kod 2")]
        public string CariKod2 { get; set; }
        [DisplayName("Kod 1")]
        public string Kod1 { get; set; }
        [DisplayName("Ödeme Günü")]
        public int? OdemeGunu { get; set; }
        [DisplayName("Vade Tarihi")]
        public DateTime? DueDate { get; set; }
        [DisplayName("Pla Kodu")]
        public string PlaKodu { get; set; }
        [DisplayName("Plasiyer Açıklama")]
        public string PlasiyerAciklama { get; set; }
        [DisplayName("GIB Fatura No")]
        public string GibFatirsNo { get; set; }
        [DisplayName("KDV Dahil mi")]
        public bool? KdvDahilmi { get; set; }
        [DisplayName("Fatura Kalem Adedi")]
        public int? FatKalemAdedi { get; set; }
        [DisplayName("Brüt Tutar")]
        public decimal Bruttutar { get; set; }
        [DisplayName("Satış İsk. T")]
        public decimal SatIskt { get; set; }
        [DisplayName("Müşteri Fazla İsk. T")]
        public decimal MfazIskt { get; set; }
        [DisplayName("Genel İsk. 1 T")]
        public decimal GenIsk1T { get; set; }
        [DisplayName("Genel İsk. 1 O")]
        public decimal GenIsk1O { get; set; }
        [DisplayName("Genel İsk. 2 T")]
        public decimal GenIsk2T { get; set; }
        [DisplayName("Genel İsk. 2 O")]
        public decimal GenIsk2O { get; set; }
        [DisplayName("Genel İsk. 3 T")]
        public decimal GenIsk3T { get; set; }
        [DisplayName("Genel İsk. 3 O")]
        public decimal GenIsk3O { get; set; }
        [DisplayName("KDV")]
        public decimal TaxAmount { get; set; }
        [DisplayName("Fatura Alt M1")]
        public string FatAltm1 { get; set; }
        [DisplayName("Fatura Alt M2")]
        public string FatAltm2 { get; set; }
        [DisplayName("Fatura Alt M3")]
        public string FatAltm3 { get; set; }
        [DisplayName("Genel Toplam")]
        public decimal GenelToplam { get; set; }
        [DisplayName("Açıklama 1")]
        public string Acik1 { get; set; }
        [DisplayName("Açıklama 2")]
        public string Acik2 { get; set; }
        [DisplayName("Açıklama 3")]
        public string Acik3 { get; set; }
        [DisplayName("Açıklama 4")]
        public string Acik4 { get; set; }
        [DisplayName("Açıklama 5")]
        public string Acik5 { get; set; }
        [DisplayName("Açıklama 6")]
        public string Acik6 { get; set; }
        [DisplayName("Açıklama 7")]
        public string Acik7 { get; set; }
        [DisplayName("Açıklama 8")]
        public string Acik8 { get; set; }
        [DisplayName("Açıklama 9")]
        public string Acik9 { get; set; }
        [DisplayName("Açıklama 10")]
        public string Acik10 { get; set; }
        [DisplayName("Açıklama 11")]
        public string Acik11 { get; set; }
        [DisplayName("Açıklama 12")]
        public string Acik12 { get; set; }
        [DisplayName("Açıklama 13")]
        public string Acik13 { get; set; }
        [DisplayName("Açıklama 14")]
        public string Acik14 { get; set; }
        [DisplayName("Açıklama 15")]
        public string Acik15 { get; set; }
        [DisplayName("Açıklama 16")]
        public string Acik16 { get; set; }
    }
}
