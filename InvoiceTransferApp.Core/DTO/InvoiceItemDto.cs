namespace InvoiceTransferApp.Core.DTO
{
    /// <summary>
    /// Fatura kalem veri aktarım nesnesi
    /// </summary>
    public class InvoiceItemDto
    {
        public int Id { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public string WarehouseCode { get; set; }
        public string Description { get; set; }
    }
}
