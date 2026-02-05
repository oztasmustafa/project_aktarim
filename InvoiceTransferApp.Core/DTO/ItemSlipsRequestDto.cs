using System;
using System.Collections.Generic;

namespace InvoiceTransferApp.Core.DTO
{
    public class ItemSlipsRequestDto
    {
        public string Seri { get; set; }
        public ItemSlipsFatUstDto FatUst { get; set; }
        public bool KayitliNumaraOtomatikGuncellensin { get; set; }
        public bool SeriliHesapla { get; set; }
        public List<ItemSlipsKalemDto> Kalems { get; set; } = new List<ItemSlipsKalemDto>();
    }

    public class ItemSlipsFatUstDto
    {
        public int Sube_Kodu { get; set; }
        public string CariKod { get; set; }
        public DateTime Tarih { get; set; }
        public DateTime? FIYATTARIHI { get; set; }
        public int Tip { get; set; }
        public int TIPI { get; set; }
        public string Proje_Kodu { get; set; }
    }

    public class ItemSlipsKalemDto
    {
        public string StokKodu { get; set; }
        public double STra_GCMIK { get; set; }
        public double STra_NF { get; set; }
        public double STra_BF { get; set; }
        public string STra_ACIKLAMA { get; set; }
        public string STra_DOVTIP { get; set; }
        public double STra_DOVFIAT { get; set; }
        public int DEPO_KODU { get; set; }
        public string ReferansKodu { get; set; }
        public string ProjeKodu { get; set; }
    }
}
