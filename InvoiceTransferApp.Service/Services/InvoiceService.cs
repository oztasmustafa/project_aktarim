using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AutoMapper;
using FluentValidation;
using InvoiceTransferApp.Core.DTO;
using InvoiceTransferApp.Core.Entities;
using InvoiceTransferApp.Core.Interfaces;
using InvoiceTransferApp.Repository.Repositories;
using InvoiceTransferApp.Service.Mapping;
using InvoiceTransferApp.Service.Validation;

namespace InvoiceTransferApp.Service.Services
{
    /// <summary>
    /// Invoice business logic service
    /// </summary>
    public class InvoiceService : IInvoiceService
    {
        private readonly INetsisRepository _netsisRepository;
        private readonly INetsisRestService _netsisRestService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly InvoiceMapper _netsisMapper;
        private readonly InvoiceValidator _validator;
        private readonly bool _useRestApi;

        /// <summary>
        /// UI için varsayılan servis oluşturur (Netsis repository, EF yok).
        /// UI projesinin Repository katmanına referans vermesini önler.
        /// </summary>
        public static IInvoiceService CreateDefault(IMapper mapper)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            var netsisRepository = new NetsisRepository();
            var netsisRestService = new NetsisRestService();
            return new InvoiceService(netsisRepository, netsisRestService, null, mapper);
        }

        public InvoiceService(
            INetsisRepository netsisRepository,
            INetsisRestService netsisRestService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _netsisRepository = netsisRepository ?? throw new ArgumentNullException(nameof(netsisRepository));
            _netsisRestService = netsisRestService;
            _unitOfWork = unitOfWork;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _netsisMapper = new InvoiceMapper();
            _validator = new InvoiceValidator();
            
            string useRestConfig = ConfigurationManager.AppSettings["UseNetsisRest"] ?? "false";
            _useRestApi = useRestConfig.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public List<InvoiceDto> GetSalesInvoices(string sourceCompany, DateTime startDate, DateTime endDate)
        {
            var entities = _netsisRepository.GetSalesInvoices(sourceCompany, startDate, endDate);
            return _mapper.Map<List<InvoiceDto>>(entities);
        }

        public InvoiceDto GetInvoiceById(int id)
        {
            var entity = _netsisRepository.GetInvoiceById(id);
            return _mapper.Map<InvoiceDto>(entity);
        }

        public string TransferInvoice(int invoiceId, string targetCompanyCode)
        {
            return TransferInvoiceAsync(invoiceId, targetCompanyCode).GetAwaiter().GetResult();
        }

        public async System.Threading.Tasks.Task<string> TransferInvoiceAsync(int invoiceId, string targetCompanyCode)
        {
            var invoice = _netsisRepository.GetInvoiceById(invoiceId);
            if (invoice == null)
                throw new ArgumentException($"Fatura bulunamadı: {invoiceId}");

            var dto = _mapper.Map<InvoiceDto>(invoice);
            if (!ValidateInvoice(dto))
                throw new ValidationException("Fatura validasyon hatası");

            var netsisFatura = _netsisMapper.MapToNetsisModel(invoice);
            
            string referenceNumber;
            
            if (_useRestApi && _netsisRestService != null)
            {
                // REST API ile transfer (yazdırma)
                var apiResult = await _netsisRestService.SaveItemSlipAsync(netsisFatura);
                if (!apiResult.Success)
                    throw new Exception(apiResult.ErrorMessage ?? "REST API ile fatura transferi başarısız!");
                
                referenceNumber = apiResult.ReferenceNumber ?? $"REST-{DateTime.Now:yyyyMMddHHmmss}";
            }
            else
            {
                // SQL ile transfer (eski yöntem)
                referenceNumber = _netsisRepository.SaveInvoice(netsisFatura, targetCompanyCode);
            }
            
            // EF kullanılıyorsa invoice'i güncelle
            invoice.IsTransferredToNetsis = true;
            invoice.TransferDate = DateTime.Now;
            invoice.NetsisReferenceNumber = referenceNumber;
            
            if (_unitOfWork != null)
            {
                _unitOfWork.Invoices.Update(invoice);
                _unitOfWork.Complete();
            }
            
            return referenceNumber;
        }

        public bool ValidateInvoice(InvoiceDto invoice)
        {
            var result = _validator.Validate(invoice);
            return result.IsValid;
        }
    }
}
