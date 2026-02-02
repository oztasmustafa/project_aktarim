using System;

namespace InvoiceTransferApp.Repository.Context
{
    /// <summary>
    /// Netsis veri bağlamı (örnek yapıdaki Context klasörüne uyum)
    /// EF Core kullanılmadığı için bağlantı bilgisi tutar.
    /// </summary>
    public class NetsisContext
    {
        public string ConnectionString { get; }

        public NetsisContext()
        {
            ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["NetsisDB"]?.ConnectionString ?? "";
        }

        public NetsisContext(string connectionString)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
    }
}
