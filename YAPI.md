# Çözüm Yapısı (Örnek Projeye Uyumlu)

Solution Explorer'da **sadece şu 4 proje** görünmelidir:

| Proje | Klasörler | Açıklama |
|-------|-----------|----------|
| **InvoiceTransferApp.Core** | DTO, Entities, Enums, Repositories, Services, UnitOfWorks, ViewModel | Entity, DTO, ViewModel, Repo arayüzleri |
| **InvoiceTransferApp.Repository** | Context, Repositories, UnitOfWorks | Veri erişimi (Netsis) |
| **InvoiceTransferApp.Service** | Helper, Mapping, Services | İş mantığı |
| **InvoiceTransferApp** | Forms, vb. | UI (WinForms) |

**Not:** Eski BLL, DAL, Entities projeleri kaldırıldı. Klasörler diskte kalmışsa (bin/obj ile) elle silebilirsiniz; Solution'da zaten yok.
