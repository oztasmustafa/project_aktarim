using System;
using System.Globalization;
using System.Linq;
using InvoiceTransferApp.Core.Entities;

namespace InvoiceTransferApp.Service.Mapping
{
    public class InvoiceMapper
    {
        public Fatura MapToNetsisModel(Invoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            if (invoice.Items == null || invoice.Items.Count == 0)
                throw new ArgumentException("Fatura kalemleri boş olamaz.", nameof(invoice));

            return new Fatura
            {
                Seri = "ALI",
                KayitliNumaraOtomatikGuncellensin = true,
                FaturaTevkifatHesaplansin = false,
                EPostaGonderilsin = false,
                StokKartinaGoreHesapla = true,
                
                FatUst = new FaturaUst
                {
                    FATIRS_NO = invoice.InvoiceNumber,
                    CariKod = invoice.CustomerCode,
                    Tarih = invoice.InvoiceDate,
                    Aciklama = invoice.Description ?? "",
                    ENTEGRE_TRH = DateTime.Now,
                    FiiliTarih = invoice.InvoiceDate,
                    SIPARIS_TEST = invoice.InvoiceDate,
                    ODEMETARIHI = invoice.DueDate ?? DateTime.Now.AddDays(30),
                    FIYATTARIHI = invoice.InvoiceDate,
                    Tip = 1,
                    TIPI = 1,
                    KDV_DAHILMI = false,
                    Sube_Kodu = 0,
                    KOD1 = null,
                    KOD2 = null,
                    PLA_KODU = null,
                    KS_KODU = null,
                    Proje_Kodu = null,
                    GIB_FATIRS_NO = null,
                    Bform = null,
                    FAT_ALTM2 = 0,
                    GEN_ISK1O = 0,
                    EKACK1 = "Otomatik Aktarım",
                    EKACK2 = "Kaynak: Erzurum_2026 Satış Fat",
                    EKACK3 = "Hedef: Bakiye Alış FAT",
                    EKACK4 = $"Aktarım: {DateTime.Now:dd.MM.yyyy HH:mm}",
                    EKACK5 = $"Kullanıcı: {Environment.UserName}",
                    EKACK6 = null,
                    EKACK7 = null,
                    EKACK8 = null,
                    EKACK9 = null,
                    EKACK10 = null,
                    EKACK11 = null,
                    EKACK12 = null,
                    EKACK13 = null,
                    EKACK14 = null,
                    EKACK15 = null,
                    EKACK16 = null,
                    EfaturaCarisiMi = false
                },
                
                Kalems = invoice.Items.Select((item, index) => new FatKalem
                {
                    StokKodu = item.ProductCode,
                    STra_GCMIK = Convert.ToDouble(item.Quantity),
                    STra_NF = item.UnitPrice.ToString("F2", CultureInfo.InvariantCulture),
                    STra_KDV = Convert.ToDouble(item.TaxRate),
                    STra_ACIK = item.Description ?? "",
                    Sira = index + 1,
                    DEPO_KODU = string.IsNullOrEmpty(item.WarehouseCode) ? 0 : 
                        (int.TryParse(item.WarehouseCode, out int depoKod) ? depoKod : 0),
                    Stok_SubeKod = 0,
                    Stra_SubeKodu = 0,
                    STra_BF = null,
                    STra_IAF = null,
                    Plasiyer_Kodu = null,
                    MuhasebeKodu = null,
                    ProjeKodu = null,
                    STra_GC = null,
                    STra_CARI_KOD = invoice.CustomerCode,
                    STra_SATISK = null,
                    STra_FTIRSIP = null,
                    STra_HTUR = null,
                    STra_BGTIP = null,
                    STra_testar = invoice.InvoiceDate,
                    Vadetar = invoice.DueDate ?? DateTime.Now.AddDays(30),
                    D_YEDEK10 = DateTime.Now,
                    Stra_OnayTipi = null,
                    Stra_OnayNum = 0,
                    Ekalanneden = null,
                    Ekalan = null
                }).ToList()
            };
        }
    }
}
