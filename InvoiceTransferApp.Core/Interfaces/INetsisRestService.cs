using System.Threading.Tasks;
using InvoiceTransferApp.Core.DTO;
using InvoiceTransferApp.Core.Entities;

namespace InvoiceTransferApp.Core.Interfaces
{
    /// <summary>
    /// Netsis REST API servisi (CCP LogoRestService uyumlu: token + ItemSlips).
    /// </summary>
    public interface INetsisRestService
    {
        SaveItemSlipResult SaveItemSlip(Fatura fatura);
        Task<SaveItemSlipResult> SaveItemSlipAsync(Fatura fatura);
        /// <summary>Token'ı geçersiz kılar (api/v2/revoke).</summary>
        bool RevokeToken(string token);
    }
}
