# Netsis Entegrasyon Rehberi

## Hızlı Başlangıç

Proje hazır! Sadece 2 metod doldurman gerekiyor.

---

## 1️⃣ Bağlantı Ayarı

📁 `App.config` dosyasını aç:

```xml
<connectionStrings>
    <add name="NetsisDB" 
         connectionString="Server=SUNUCU;Database=NETSIS;User Id=KULLANICI;Password=SIFRE;TrustServerCertificate=True" />
</connectionStrings>
```

---

## 2️⃣ GetSalesInvoicesFromNetsis() - Veri Çekme

📁 `Services/NetsisService.cs` → Satır ~28

### Ne Yapmalısın?

```csharp
public List<Invoice> GetSalesInvoicesFromNetsis(string sourceCompany, DateTime startDate, DateTime endDate)
{
    var invoices = new List<Invoice>();
    
    using (var conn = new SqlConnection(_connectionString))
    {
        conn.Open();
        
        // 1. FATURA_UST tablosundan başlıkları çek
        var cmd = new SqlCommand(@"
            SELECT 
                FATIRS_NO, CariKod, Tarih, Aciklama, Tutar, VadeTarihi
            FROM FATURA_UST
            WHERE Tarih BETWEEN @StartDate AND @EndDate
                AND Tip = 2  -- Satış faturası (doğru tip numarasını kontrol et)
            ORDER BY Tarih DESC
        ", conn);
        
        cmd.Parameters.AddWithValue("@StartDate", startDate);
        cmd.Parameters.AddWithValue("@EndDate", endDate);
        
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var invoice = new Invoice
                {
                    Id = invoices.Count + 1,
                    InvoiceNumber = reader["FATIRS_NO"].ToString(),
                    CustomerCode = reader["CariKod"].ToString(),
                    InvoiceDate = Convert.ToDateTime(reader["Tarih"]),
                    Description = reader["Aciklama"].ToString(),
                    TotalAmount = Convert.ToDecimal(reader["Tutar"]),
                    DueDate = reader["VadeTarihi"] != DBNull.Value ? Convert.ToDateTime(reader["VadeTarihi"]) : (DateTime?)null,
                    Currency = "TRY",
                    CompanyCode = sourceCompany
                };
                
                invoices.Add(invoice);
            }
        }
        
        // 2. Her fatura için kalemleri çek
        foreach (var invoice in invoices)
        {
            invoice.Items = GetInvoiceItems(conn, invoice.InvoiceNumber);
        }
    }
    
    return invoices;
}

// Yardımcı metod ekle
private List<InvoiceItem> GetInvoiceItems(SqlConnection conn, string invoiceNumber)
{
    var items = new List<InvoiceItem>();
    
    var cmd = new SqlCommand(@"
        SELECT 
            StokKodu, STra_GCMIK, STra_NF, STra_KDV, STra_ACIK, DEPO_KODU
        FROM FATURA_KALEM
        WHERE FATIRS_NO = @FatNo
        ORDER BY Sira
    ", conn);
    
    cmd.Parameters.AddWithValue("@FatNo", invoiceNumber);
    
    using (var reader = cmd.ExecuteReader())
    {
        int id = 1;
        while (reader.Read())
        {
            decimal quantity = Convert.ToDecimal(reader["STra_GCMIK"]);
            decimal unitPrice = decimal.Parse(reader["STra_NF"].ToString());
            decimal taxRate = Convert.ToDecimal(reader["STra_KDV"]);
            decimal lineTotal = quantity * unitPrice;
            decimal taxAmount = lineTotal * (taxRate / 100);
            
            items.Add(new InvoiceItem
            {
                Id = id++,
                ProductCode = reader["StokKodu"].ToString(),
                Quantity = quantity,
                UnitPrice = unitPrice,
                LineTotal = lineTotal,
                TaxRate = taxRate,
                TaxAmount = taxAmount,
                WarehouseCode = reader["DEPO_KODU"].ToString(),
                Description = reader["STra_ACIK"].ToString()
            });
        }
    }
    
    return items;
}
```

---

## 3️⃣ SaveToNetsis() - Veri Yazma

📁 `Services/NetsisService.cs` → Satır ~66

### Ne Yapmalısın?

```csharp
private string SaveToNetsis(Fatura fatura, string targetCompanyCode)
{
    using (var conn = new SqlConnection(_connectionString))
    {
        conn.Open();
        
        using (var transaction = conn.BeginTransaction())
        {
            try
            {
                // 1. FATURA_UST'a ekle
                var cmdHeader = new SqlCommand(@"
                    INSERT INTO FATURA_UST (
                        FATIRS_NO, CariKod, Tarih, Aciklama, Tip, TIPI,
                        ENTEGRE_TRH, FiiliTarih, SIPARIS_TEST, ODEMETARIHI, FIYATTARIHI,
                        KDV_DAHILMI, Sube_Kodu, EKACK1, EKACK2, EKACK3, EKACK4, EKACK5
                    )
                    VALUES (
                        @FatNo, @CariKod, @Tarih, @Aciklama, @Tip, @TIPI,
                        @EntegreTrh, @FiiliTarih, @SiparisTarih, @OdemeTarihi, @FiyatTarihi,
                        @KdvDahilMi, @SubeKodu, @Ekack1, @Ekack2, @Ekack3, @Ekack4, @Ekack5
                    )
                ", conn, transaction);
                
                cmdHeader.Parameters.AddWithValue("@FatNo", fatura.FatUst.FATIRS_NO);
                cmdHeader.Parameters.AddWithValue("@CariKod", fatura.FatUst.CariKod);
                cmdHeader.Parameters.AddWithValue("@Tarih", fatura.FatUst.Tarih);
                cmdHeader.Parameters.AddWithValue("@Aciklama", fatura.FatUst.Aciklama ?? "");
                cmdHeader.Parameters.AddWithValue("@Tip", fatura.FatUst.Tip);
                cmdHeader.Parameters.AddWithValue("@TIPI", fatura.FatUst.TIPI);
                cmdHeader.Parameters.AddWithValue("@EntegreTrh", fatura.FatUst.ENTEGRE_TRH);
                cmdHeader.Parameters.AddWithValue("@FiiliTarih", fatura.FatUst.FiiliTarih);
                cmdHeader.Parameters.AddWithValue("@SiparisTarih", fatura.FatUst.SIPARIS_TEST);
                cmdHeader.Parameters.AddWithValue("@OdemeTarihi", fatura.FatUst.ODEMETARIHI);
                cmdHeader.Parameters.AddWithValue("@FiyatTarihi", fatura.FatUst.FIYATTARIHI);
                cmdHeader.Parameters.AddWithValue("@KdvDahilMi", fatura.FatUst.KDV_DAHILMI);
                cmdHeader.Parameters.AddWithValue("@SubeKodu", fatura.FatUst.Sube_Kodu);
                cmdHeader.Parameters.AddWithValue("@Ekack1", fatura.FatUst.EKACK1 ?? "");
                cmdHeader.Parameters.AddWithValue("@Ekack2", fatura.FatUst.EKACK2 ?? "");
                cmdHeader.Parameters.AddWithValue("@Ekack3", fatura.FatUst.EKACK3 ?? "");
                cmdHeader.Parameters.AddWithValue("@Ekack4", fatura.FatUst.EKACK4 ?? "");
                cmdHeader.Parameters.AddWithValue("@Ekack5", fatura.FatUst.EKACK5 ?? "");
                
                cmdHeader.ExecuteNonQuery();
                
                // 2. Her kalem için FATURA_KALEM'e ekle
                foreach (var kalem in fatura.Kalems)
                {
                    var cmdItem = new SqlCommand(@"
                        INSERT INTO FATURA_KALEM (
                            FATIRS_NO, StokKodu, STra_GCMIK, STra_NF, STra_KDV,
                            STra_ACIK, Sira, DEPO_KODU, STra_CARI_KOD,
                            STra_testar, Vadetar, D_YEDEK10
                        )
                        VALUES (
                            @FatNo, @StokKod, @Miktar, @BirimFiyat, @Kdv,
                            @Aciklama, @Sira, @DepoKod, @CariKod,
                            @Tarih, @VadeTarih, @YedekTarih
                        )
                    ", conn, transaction);
                    
                    cmdItem.Parameters.AddWithValue("@FatNo", fatura.FatUst.FATIRS_NO);
                    cmdItem.Parameters.AddWithValue("@StokKod", kalem.StokKodu);
                    cmdItem.Parameters.AddWithValue("@Miktar", kalem.STra_GCMIK);
                    cmdItem.Parameters.AddWithValue("@BirimFiyat", kalem.STra_NF);
                    cmdItem.Parameters.AddWithValue("@Kdv", kalem.STra_KDV);
                    cmdItem.Parameters.AddWithValue("@Aciklama", kalem.STra_ACIK ?? "");
                    cmdItem.Parameters.AddWithValue("@Sira", kalem.Sira);
                    cmdItem.Parameters.AddWithValue("@DepoKod", kalem.DEPO_KODU);
                    cmdItem.Parameters.AddWithValue("@CariKod", kalem.STra_CARI_KOD);
                    cmdItem.Parameters.AddWithValue("@Tarih", kalem.STra_testar);
                    cmdItem.Parameters.AddWithValue("@VadeTarih", kalem.Vadetar);
                    cmdItem.Parameters.AddWithValue("@YedekTarih", kalem.D_YEDEK10);
                    
                    cmdItem.ExecuteNonQuery();
                }
                
                transaction.Commit();
                return fatura.FatUst.FATIRS_NO;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Netsis kayıt hatası: {ex.Message}", ex);
            }
        }
    }
}
```

---

## 4️⃣ Test

```bash
dotnet run
```

1. "Getir" butonu → Faturalar listelenmeli
2. Fatura seç → "Aktar" butonu
3. Netsis'te kontrol et

---

## ⚠️ Önemli Notlar

- **Tablo isimleri:** Gerçek Netsis tablo isimlerini kontrol et
- **Kolon isimleri:** SQL sorgularında kolonları kontrol et
- **Tip değerleri:** Satış (Tip=2?), Alış (Tip=1?) değerlerini doğrula
- **Seri kodu:** "ALI" doğru mu? (`NetsisService.cs` satır ~142)
- **Fatura numarası:** Otomatik mi, manuel mi?

---

## 📞 Sorular?

Mapping hazır, UI hazır. Sadece SQL kısmını doldur!

**Başarılar!** 🚀

---

# Entegrasyon için neler gerekli / neler yaptık

Bu bölüm, entegrasyonun ne gerektirdiğini ve projede **hangi adımların tamamlandığını** özetler.

---

## Entegre için neler lazım?

| Gereksinim | Açıklama |
|------------|----------|
| **Veri alma (okuma)** | Netsis veritabanından fatura listesi ve fatura detayı çekmek |
| **Veri yazma (aktarma)** | Seçilen faturaları hedef şirkete Netsis’e yazmak (TBLFATUIRS + TBLSTHAR) |
| **Bağlantı** | Netsis SQL Server bağlantı dizesi (tek merkez: App.config veya API appsettings) |
| **İsteğe bağlı: API** | Uygulama API üzerinden veri alıp yazacaksa: HTTP endpoint’leri (GET/POST) |

---

## Biz neler yaptık? (Entegre için yapılanlar)

### 1. Veri alma (okuma) – **YAPILDI**

- **Nerede:** `InvoiceTransferApp.Repository` → `NetsisRepository.cs`
- **Metotlar:**
  - **`GetSalesInvoices(sourceCompany, startDate, endDate)`**  
    Tarih aralığına göre satış faturalarını **TBLFATUIRS** (+ JOIN’ler) üzerinden çeker. Sonuç `List<Invoice>` döner.
  - **`GetInvoiceById(id)`**  
    Önceden çekilmiş listeden faturayı bulur; kalemleri **TBLSTHAR** üzerinden **FISNO** ile çeker.
- **Kullanılan tablolar:** TBLFATUIRS, TBLSTHAR, TBLCASABIT, TBLFATUEK, TBLCARIPLASIYER, TBLSTSABIT.
- **Bağlantı:** `NetsisDB` connection string (App.config / API appsettings).

### 2. Veri yazma (Netsis’e aktarma) – **YAPILDI**

- **Nerede:** `InvoiceTransferApp.Repository` → `NetsisRepository.cs`
- **Metot:** **`SaveInvoice(Fatura fatura, string targetCompanyCode)`**
  - Fatura başlığını **TBLFATUIRS**’e INSERT eder (FTIRSIP='2' = alış faturası).
  - Her kalemi **TBLSTHAR**’a INSERT eder (STHAR_FTIRSIP='2', FISNO, STOK_KODU, STHAR_GCMIK, STHAR_NF, vb.).
  - Transaction kullanır; hata olursa rollback.
  - `targetCompanyCode` sayıysa **SUBE_KODU** olarak kullanılır.
- **Çağıran yer:** `InvoiceService.TransferInvoice(invoiceId, targetCompanyCode)` → faturayı alır, `Fatura` modeline map’ler, `SaveInvoice` çağırır.

### 3. API (entegreye hazır) – **YAPILDI**

- **Proje:** `InvoiceTransferApp.API` (ASP.NET Core Web API)
- **Endpoint’ler:**

| Metot | URL | Açıklama |
|-------|-----|----------|
| **GET** | `/api/invoices?sourceCompany=...&startDate=...&endDate=...` | Fatura listesi (veri alma) |
| **GET** | `/api/invoices/{id}` | Tek fatura detayı (kalemler dahil) |
| **POST** | `/api/invoices/transfer` | Faturayı hedef şirkete aktarır (yazma). Body: `{ "invoiceId": ..., "targetCompanyCode": "..." }` |

- **Bağlantı:** API’nin `appsettings.json` → **ConnectionStrings:NetsisDB**
- **Çalıştırma:** `cd InvoiceTransferApp.API` → `dotnet run` (Swagger: https://localhost:5001/swagger)

### 4. Masaüstü uygulama – **ENTEGRE HAZIR**

- **Proje:** `InvoiceTransferApp.UI` (WinForms)
- **Akış:**
  - **Getir:** `GetSalesInvoices` → grid’e listeler.
  - **Aktar:** Seçilen faturalar için `TransferInvoice` → `SaveInvoice` ile Netsis’e yazar.
- **Bağlantı:** `InvoiceTransferApp.UI` → `App.config` → **NetsisDB**

---

## Entegre için method özeti

| İş | Metot | Dosya / Katman |
|----|--------|-----------------|
| **Fatura listesi al** | `NetsisRepository.GetSalesInvoices(...)` | Repository/NetsisRepository.cs |
| **Fatura detay al** | `NetsisRepository.GetInvoiceById(id)` + kalemler TBLSTHAR | Aynı dosya |
| **Netsis’e yaz** | `NetsisRepository.SaveInvoice(Fatura, targetCompanyCode)` | Aynı dosya |
| **İş mantığı (liste)** | `InvoiceService.GetSalesInvoices(...)` | Service/InvoiceService.cs |
| **İş mantığı (detay)** | `InvoiceService.GetInvoiceById(id)` | Aynı dosya |
| **İş mantığı (aktar)** | `InvoiceService.TransferInvoice(invoiceId, targetCompanyCode)` | Aynı dosya |
| **API – liste** | `GET /api/invoices` → Service.GetSalesInvoices | API/Controllers/InvoicesController.cs |
| **API – detay** | `GET /api/invoices/{id}` → Service.GetInvoiceById | Aynı dosya |
| **API – aktar** | `POST /api/invoices/transfer` → Service.TransferInvoice | Aynı dosya |

---

## Kısa özet

- **Entegre için gerekli:** Veri alma + veri yazma + bağlantı; isteğe bağlı API.
- **Yapılanlar:** Veri alma (GetSalesInvoices, GetInvoiceById), veri yazma (SaveInvoice – TBLFATUIRS + TBLSTHAR), API endpoint’leri (GET/POST), masaüstü Getir/Aktar akışı. Bağlantı App.config ve API appsettings’te.
- **Methodlar:** Yukarıdaki tabloda; Repository’de SQL, Service’de iş kuralları, API’de HTTP.
