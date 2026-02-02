using System;
using System.Collections.Generic;

namespace InvoiceTransferApp.Core.Entities
{
    public class Fatura
    {
        public string Seri { get; set; }
        public bool KayitliNumaraOtomatikGuncellensin { get; set; }
        public bool FaturaTevkifatHesaplansin { get; set; }
        public bool EPostaGonderilsin { get; set; }
        public bool StokKartinaGoreHesapla { get; set; }
        
        public FaturaUst FatUst { get; set; }
        public List<FatKalem> Kalems { get; set; }

        public Fatura()
        {
            Kalems = new List<FatKalem>();
        }
    }

    public class FaturaUst
    {
        public string FATIRS_NO { get; set; }
        public string CariKod { get; set; }
        public DateTime Tarih { get; set; }
        public DateTime ENTEGRE_TRH { get; set; }
        public DateTime FiiliTarih { get; set; }
        public DateTime SIPARIS_TEST { get; set; }
        public DateTime ODEMETARIHI { get; set; }
        public DateTime FIYATTARIHI { get; set; }
        public string Aciklama { get; set; }
        public int Tip { get; set; }
        public string KOD1 { get; set; }
        public string KOD2 { get; set; }
        public decimal FAT_ALTM2 { get; set; }
        public double GEN_ISK1O { get; set; }
        public int TIPI { get; set; }
        public bool KDV_DAHILMI { get; set; }
        public string PLA_KODU { get; set; }
        public string KS_KODU { get; set; }
        public string Proje_Kodu { get; set; }
        public string GIB_FATIRS_NO { get; set; }
        public int Sube_Kodu { get; set; }
        public string Bform { get; set; }
        
        public string EKACK1 { get; set; }
        public string EKACK2 { get; set; }
        public string EKACK3 { get; set; }
        public string EKACK4 { get; set; }
        public string EKACK5 { get; set; }
        public string EKACK6 { get; set; }
        public string EKACK7 { get; set; }
        public string EKACK8 { get; set; }
        public string EKACK9 { get; set; }
        public string EKACK10 { get; set; }
        public string EKACK11 { get; set; }
        public string EKACK12 { get; set; }
        public string EKACK13 { get; set; }
        public string EKACK14 { get; set; }
        public string EKACK15 { get; set; }
        public string EKACK16 { get; set; }
        
        public bool EfaturaCarisiMi { get; set; }
    }

    public class FatKalem
    {
        public string StokKodu { get; set; }
        public double STra_GCMIK { get; set; }
        public string STra_NF { get; set; }
        public string STra_BF { get; set; }
        public string STra_IAF { get; set; }
        public string Plasiyer_Kodu { get; set; }
        public int DEPO_KODU { get; set; }
        public int Stok_SubeKod { get; set; }
        public int Stra_SubeKodu { get; set; }
        public string STra_SATISK { get; set; }
        public double STra_KDV { get; set; }
        public string MuhasebeKodu { get; set; }
        public string ProjeKodu { get; set; }
        public string STra_GC { get; set; }
        public string STra_ACIK { get; set; }
        public string STra_CARI_KOD { get; set; }
        public string STra_FTIRSIP { get; set; }
        public string STra_HTUR { get; set; }
        public string STra_BGTIP { get; set; }
        public int Sira { get; set; }
        public DateTime STra_testar { get; set; }
        public DateTime Vadetar { get; set; }
        public DateTime D_YEDEK10 { get; set; }
        public string Stra_OnayTipi { get; set; }
        public int Stra_OnayNum { get; set; }
        public string Ekalanneden { get; set; }
        public string Ekalan { get; set; }
    }
}
