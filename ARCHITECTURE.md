# Kurumsal Katmanlı Mimari - InvoiceTransferApp

## Çözüm Yapısı

```
InvoiceTransferApp.sln
│
├── InvoiceTransferApp.Core          # Domain Layer (hiçbir projeye bağımlı değil)
│   ├── Entities/                    # Domain modelleri (Invoice, InvoiceItem, NetsisModels)
│   ├── DTOs/                        # Data Transfer Objects (katmanlar arası veri taşıma)
│   ├── ViewModel/                   # UI binding modelleri
│   ├── Interfaces/                  # Tüm interface'ler (IRepository, IService, IUnitOfWork)
│   ├── Enums/                       # Enum tanımları
│   └── Mapping/                     # AutoMapper profilleri
│
├── InvoiceTransferApp.Repository    # Data Access Layer (Core'a bağımlı)
│   ├── Context/                     # DbContext, DbContextFactory
│   ├── Migrations/                  # EF Core migrations (dotnet ef migrations add)
│   ├── Repositories/                # Repository implementasyonları
│   └── UnitOfWorks/                 # UnitOfWork implementasyonu
│
├── InvoiceTransferApp.Service       # Business Logic Layer (Core + Repository'e bağımlı)
│   ├── Services/                    # Business logic (InvoiceService)
│   ├── Validation/                  # FluentValidation validators
│   ├── Mapping/                     # Custom mapping (Netsis modeline çevrim)
│   └── Helper/                      # Yardımcı sınıflar
│
└── InvoiceTransferApp.UI            # Presentation Layer - Masaüstü (Service + Core'a bağımlı)
    ├── Forms/                       # WinForms (InvoiceDetailForm vb.)
    ├── Program.cs                   # Entry point
    └── MainForm.cs                  # Ana form
```

## Bağımlılık Kuralları ✅

1. **Core** → Hiçbir projeye bağımlı değil (saf domain)
2. **Repository** → Yalnızca Core'a bağımlı
3. **Service** → Core + Repository'e bağımlı
4. **UI** → Service + Core'a bağımlı (Repository'yi bilmez)

## Veri Akışı

```
UI (WinForms)
  ↓ (InvoiceDto/ViewModel)
Service (Business Logic + Validation)
  ↓ (Entity)
Repository (Data Access - Netsis/EF Core)
  ↓
Database (Netsis / Local DB)
```

## Kullanılan Teknolojiler

- **.NET 10.0** - Framework
- **AutoMapper 13.0.1** - Entity ↔ DTO ↔ ViewModel mapping
- **FluentValidation 11.11.0** - DTO validasyonu
- **EF Core 9.0.0** - ORM (opsiyonel, Netsis yanında kullanılabilir)
- **DevExpress WinForms 25.1.3** - UI bileşenleri

## Kullanım Örnekleri

### Service kullanımı (UI katmanında)

```csharp
// DI ile service oluşturulur
IInvoiceService service = new InvoiceService(netsisRepo, unitOfWork, mapper);

// Fatura listesi (DTO döner)
var invoices = service.GetSalesInvoices("Şirket", startDate, endDate);

// Fatura aktarımı
string refNo = service.TransferInvoice(invoiceId, "Hedef Şirket");
```

### AutoMapper kullanımı

```csharp
var config = new MapperConfiguration(cfg => 
    cfg.AddProfile<InvoiceTransferApp.Core.Mapping.MappingProfile>());
IMapper mapper = config.CreateMapper();

// Entity -> DTO
InvoiceDto dto = mapper.Map<InvoiceDto>(entity);

// DTO -> ViewModel (UI binding için)
InvoiceListViewModel vm = mapper.Map<InvoiceListViewModel>(dto);
```

## Migration Oluşturma (EF Core)

```bash
cd InvoiceTransferApp.Repository
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Test Edilebilirlik

- Core sadece interface'ler ve POCO'lar içerdiği için mock'lanabilir
- Service testleri için INetsisRepository ve IUnitOfWork mock'lanabilir
- UI testleri için IInvoiceService mock'lanabilir

## Genişletilebilirlik

- **Yeni entity:** Core/Entities'e ekle → Repository/Repository ekle → Service/Service ekle
- **Yeni entegrasyon:** Core/Interfaces'e INewSystemRepository ekle → Repository'de implement et
- **Yeni UI:** Service katmanını kullanarak (WinForms, Web API, Blazor) bağlanabilir

## Notlar

- Netsis entegrasyonu şu an `NotImplementedException` fırlatıyor; implementasyon eklenecek
- EF Core DbContext opsiyonel; Netsis yanında local cache için kullanılabilir
- PanelUserDto ile oturum yönetimi detay formunda görüntülenir
