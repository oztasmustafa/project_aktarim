using System;
using System.Collections.Generic;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly InvoiceMapper _netsisMapper;
        private readonly InvoiceValidator _validator;

        /// <summary>
        /// UI için varsayılan servis oluşturur (Netsis repository, EF yok).
        /// UI projesinin Repository katmanına referans vermesini önler.
        /// </summary>
        public static IInvoiceService CreateDefault(IMapper mapper)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            var netsisRepository = new NetsisRepository();
            return new InvoiceService(netsisRepository, null, mapper);
        }

        public InvoiceService(
            INetsisRepository netsisRepository, 
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _netsisRepository = netsisRepository ?? throw new ArgumentNullException(nameof(netsisRepository));
            _unitOfWork = unitOfWork;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _netsisMapper = new InvoiceMapper();
            _validator = new InvoiceValidator();
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
            var invoice = _netsisRepository.GetInvoiceById(invoiceId);
            if (invoice == null)
                throw new ArgumentException($"Fatura bulunamadı: {invoiceId}");

            var dto = _mapper.Map<InvoiceDto>(invoice);
            if (!ValidateInvoice(dto))
                throw new ValidationException("Fatura validasyon hatası");

            var netsisFatura = _netsisMapper.MapToNetsisModel(invoice);
            string referenceNumber = _netsisRepository.SaveInvoice(netsisFatura, targetCompanyCode);
            
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
