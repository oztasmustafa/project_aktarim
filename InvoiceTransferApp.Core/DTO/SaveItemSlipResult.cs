namespace InvoiceTransferApp.Core.DTO
{
    /// <summary>
    /// Netsis ItemSlips API çağrısı sonucu.
    /// </summary>
    public class SaveItemSlipResult
    {
        public bool Success { get; set; }
        /// <summary>Oluşan fiş/fatura numarası (API'den dönerse).</summary>
        public string ReferenceNumber { get; set; }
        /// <summary>Hata açıklaması (başarısızsa).</summary>
        public string ErrorMessage { get; set; }
    }
}
