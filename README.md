# Fatura Aktarım Uygulaması

Windows Desktop uygulaması - Satış faturalarını alış faturası olarak Netsis ERP sistemine aktarır.

## Teknolojiler

- **.NET 10.0** - Windows Desktop Framework
- **DevExpress WinForms** - UI komponenti
- **C#** - Programlama dili
- **SQL Server** - Netsis veritabanı

## Proje Yapısı

```
InvoiceTransferApp/
├── MainForm.cs              # Ana form (UI)
├── Models/
│   ├── Invoice.cs           # Uygulama fatura modeli
│   └── NetsisModels.cs      # Netsis model yapıları (Fatura, FaturaUst, FatKalem)
├── Services/
│   └── NetsisService.cs     # Netsis entegrasyon servisi
├── Forms/
│   └── InvoiceDetailForm.cs # Fatura detay popup
└── App.config               # Veritabanı bağlantı ayarları
```

## Özellikler

- ✅ Satış faturalarını tarih aralığına göre listeleme
- ✅ Fatura ve kalem detaylarını görüntüleme
- ✅ Çoklu fatura seçimi ve aktarım
- ✅ Durum filtreleme (Tümü/Aktarılmayan/Aktarılan)
- ✅ Genel arama (Fatura no, cari kod, cari adı)
- ✅ Netsis model mapping (hazır)

## Kurulum

### 1. Gereksinimler

- Visual Studio 2022 veya üzeri
- .NET 10.0 SDK
- SQL Server (Netsis veritabanına erişim)

### 2. Bağlantı Ayarları

`App.config` dosyasını düzenleyin:

```xml
<connectionStrings>
    <add name="NetsisDB" 
         connectionString="Server=SUNUCU_ADI;Database=NETSIS;User Id=KULLANICI;Password=SIFRE;TrustServerCertificate=True" />
</connectionStrings>
```

### 3. Build ve Çalıştırma

```bash
dotnet build
dotnet run
```

## Netsis Entegrasyonu

### Yapılması Gerekenler

**`NetsisService.cs`** içinde 2 metod tamamlanmalı:

#### 1. GetSalesInvoicesFromNetsis()

Satış faturalarını Netsis veritabanından çeker.

```csharp
// SQL ile FATURA_UST ve FATURA_KALEM tablolarından veri çekin
// Invoice listesi olarak döndürün
```

#### 2. SaveToNetsis()

Alış faturasını Netsis veritabanına yazar.

```csharp
// Transaction başlat
// FATURA_UST'a INSERT
// Her kalem için FATURA_KALEM'e INSERT
// Transaction commit
```

### Mapping

`MapToNetsisModel()` metodu **hazır ve çalışıyor**.

Uygulama `Invoice` modelini Netsis `Fatura` modeline otomatik dönüştürür:

- `Invoice` → `Fatura`
- `Invoice.Items` → `Fatura.Kalems`
- Tüm veri tipi dönüşümleri (decimal→double, decimal→string)
- Tarih ve alan mapping'leri

### Netsis Model Yapısı

```csharp
Fatura
├── Seri (string)
├── FatUst (FaturaUst)
│   ├── FATIRS_NO
│   ├── CariKod
│   ├── Tarih
│   └── ... (50+ alan)
└── Kalems (List<FatKalem>)
    ├── StokKodu
    ├── STra_GCMIK (double)
    ├── STra_NF (string)
    └── ... (30+ alan)
```

## Sabit Değerler

- **Kaynak Şirket:** Erzurum_2026 Satış Fat
- **Hedef Şirket:** Bakiye Alış FAT
- **Fatura Serisi:** ALI (alış faturası)
- **Fatura Tipi:** 1

## Kullanım

1. Uygulama açıldığında varsayılan tarih aralığı görüntülenir
2. **"Getir"** butonu ile satış faturaları listelenir
3. Grid'den faturalar seçilir (checkbox)
4. **"Aktar"** butonu ile seçili faturalar Netsis'e aktarılır
5. Durum kolonu güncellenir (Aktarılan/Aktarılmayan)

## Notlar

- Faturaya çift tıklayarak detay görüntülenebilir
- Aktarılan faturalar detayda aktarım tarihi gösterir
- Hata durumunda detaylı mesaj gösterilir

## Lisans

Bu proje şirket içi kullanım içindir.
