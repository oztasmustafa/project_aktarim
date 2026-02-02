using FluentValidation;
using InvoiceTransferApp.Core.DTO;

namespace InvoiceTransferApp.Service.Validation
{
    /// <summary>
    /// Invoice DTO validasyonu
    /// </summary>
    public class InvoiceValidator : AbstractValidator<InvoiceDto>
    {
        public InvoiceValidator()
        {
            RuleFor(x => x.InvoiceNumber)
                .NotEmpty().WithMessage("Fatura numarası boş olamaz")
                .MaximumLength(50).WithMessage("Fatura numarası en fazla 50 karakter olabilir");

            RuleFor(x => x.CustomerCode)
                .NotEmpty().WithMessage("Cari kodu boş olamaz");

            RuleFor(x => x.InvoiceDate)
                .NotEmpty().WithMessage("Fatura tarihi boş olamaz")
                .LessThanOrEqualTo(System.DateTime.Now).WithMessage("Fatura tarihi gelecek olamaz");

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0).WithMessage("Toplam tutar sıfırdan büyük olmalıdır");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Fatura kalemleri boş olamaz");
        }
    }
}
