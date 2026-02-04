# Netsis REST API Entegrasyonu

Bu dokümanda Netsis REST API ile fiş/fatura yazdırma (ItemSlips) akışı özetlenir.

## Ayar (App.config)

```xml
<add key="UseNetsisRest" value="true" />
<add key="NetsisRestUrl" value="http://135.181.90.228:6969" />
<add key="NetsisRestUsername" value="..." />
<add key="NetsisRestPassword" value="..." />
<add key="NetsisRestGrantType" value="password" />
<add key="NetsisRestBranchCode" value="0" />
<add key="NetsisRestDbName" value="..." />
<add key="NetsisRestDbUser" value="..." />
<add key="NetsisRestDbPassword" value="..." />
<add key="NetsisRestDbType" value="0" />
```

## Akış

1. **Token:** `POST {baseUrl}/api/v2/token`  
   - Content-Type: `application/x-www-form-urlencoded`  
   - Body: `grant_type`, `username`, `password`; isteğe bağlı: `branchcode`, `dbname`, `dbuser`, `dbpassword`, `dbtype`  
   - Başarılı yanıtta `access_token` ve `expires_in` kullanılır.

2. **ItemSlips (yazdırma):** `POST {baseUrl}/api/v2/ItemSlips`  
   - Header: `Authorization: Bearer {token}`  
   - Content-Type: `application/json`  
   - Body: camelCase JSON (seri, fatUst, kalems, kayitliNumaraOtomatikGuncellensin, seriliHesapla, sipdepokodkullan).  
   - Tarihler: `yyyy-MM-ddTHH:mm:ss` formatında gönderilir.

## Debug Dosyaları

Çalışma klasöründe (genelde exe yanında) oluşur:

- **NetsisToken.txt** – Token isteği/yanıtı; hata durumunda sunucu yanıt gövdesi burada.
- **NetsisItemSlipsRequest.txt** – API'ye gönderilen JSON.
- **NetsisItemSlipsResponse.txt** – API'den dönen HTTP durumu ve gövde.

Token 500 vb. alıyorsanız `NetsisToken.txt` içindeki **Body** satırına bakın; sunucu hata mesajını orada verir.

## API Yanıtı

- Başarı: `IsSuccessful: true`; fiş numarası varsa `Data.FisNo`, `Data.KayitNo` veya `Data.ReferenceNumber` kullanılır.
- Hata: `IsSuccessful: false` veya `ErrorDesc`; bu mesaj kullanıcıya iletilir.

## Kod Konumları

- Token + ItemSlips isteği: `InvoiceTransferApp.Service` → `NetsisRestService.cs`
- DTO: `InvoiceTransferApp.Core` → `DTO/ItemSlipsRequestDto.cs`, `SaveItemSlipResult.cs`
- İş akışı: `InvoiceService.TransferInvoiceAsync` → `NetsisRestService.SaveItemSlipAsync`

API’nizde alan adları veya endpoint farklıysa (ör. PascalCase, farklı URL) sadece bu servis ve DTO’lar güncellenir.
