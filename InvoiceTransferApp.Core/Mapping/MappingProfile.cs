using AutoMapper;
using InvoiceTransferApp.Core.DTO;
using InvoiceTransferApp.Core.Entities;
using InvoiceTransferApp.Core.ViewModel;

namespace InvoiceTransferApp.Core.Mapping
{
    /// <summary>
    /// AutoMapper profili - Entity <-> DTO <-> ViewModel mapping kuralları
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entity -> DTO
            CreateMap<Invoice, InvoiceDto>();
            CreateMap<InvoiceItem, InvoiceItemDto>();
            
            // DTO -> Entity
            CreateMap<InvoiceDto, Invoice>();
            CreateMap<InvoiceItemDto, InvoiceItem>();
            
            // Entity -> ViewModel
            CreateMap<Invoice, InvoiceListViewModel>();
            CreateMap<Invoice, InvoiceDetailViewModel>();
            CreateMap<InvoiceItem, InvoiceItemDetailViewModel>()
                .ForMember(dest => dest.ItemNo, opt => opt.Ignore());
            
            // DTO -> ViewModel
            CreateMap<InvoiceDto, InvoiceListViewModel>();
            CreateMap<InvoiceDto, InvoiceDetailViewModel>();
            CreateMap<InvoiceItemDto, InvoiceItemDetailViewModel>()
                .ForMember(dest => dest.ItemNo, opt => opt.Ignore());
        }
    }
}
