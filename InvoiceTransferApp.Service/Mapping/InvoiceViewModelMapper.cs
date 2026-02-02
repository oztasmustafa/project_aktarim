using System;
using System.Collections.Generic;
using System.Linq;
using InvoiceTransferApp.Core.Entities;
using InvoiceTransferApp.Core.ViewModel;

namespace InvoiceTransferApp.Service.Mapping
{
    /// <summary>
    /// Entity -> ViewModel mapping (Core ViewModel kullanımı için)
    /// </summary>
    public class InvoiceViewModelMapper
    {
        public InvoiceListViewModel ToListViewModel(Invoice entity)
        {
            if (entity == null) return null;
            return new InvoiceListViewModel
            {
                Id = entity.Id,
                InvoiceNumber = entity.InvoiceNumber,
                InvoiceDate = entity.InvoiceDate,
                CustomerCode = entity.CustomerCode,
                CustomerName = entity.CustomerName,
                TotalAmount = entity.TotalAmount,
                Currency = entity.Currency,
                Description = entity.Description,
                Status = entity.Status,
                IsTransferredToNetsis = entity.IsTransferredToNetsis,
                TransferDate = entity.TransferDate,
                NetsisReferenceNumber = entity.NetsisReferenceNumber
            };
        }

        public List<InvoiceListViewModel> ToListViewModelList(List<Invoice> entities)
        {
            if (entities == null) return new List<InvoiceListViewModel>();
            return entities.Select(ToListViewModel).ToList();
        }

        public InvoiceDetailViewModel ToDetailViewModel(Invoice entity)
        {
            if (entity == null) return null;
            var vm = new InvoiceDetailViewModel
            {
                Id = entity.Id,
                InvoiceNumber = entity.InvoiceNumber,
                InvoiceDate = entity.InvoiceDate,
                CustomerCode = entity.CustomerCode,
                CustomerName = entity.CustomerName,
                TotalAmount = entity.TotalAmount,
                Currency = entity.Currency,
                Description = entity.Description,
                IsTransferredToNetsis = entity.IsTransferredToNetsis,
                TransferDate = entity.TransferDate,
                NetsisReferenceNumber = entity.NetsisReferenceNumber
            };
            if (entity.Items != null)
            {
                int sira = 1;
                foreach (var item in entity.Items)
                {
                    vm.Items.Add(new InvoiceItemDetailViewModel
                    {
                        Id = item.Id,
                        ItemNo = sira++,
                        ProductCode = item.ProductCode,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal,
                        TaxRate = item.TaxRate,
                        TaxAmount = item.TaxAmount
                    });
                }
            }
            return vm;
        }
    }
}
