using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoiceTransferApp.Repository.Context
{
    /// <summary>
    /// Design-time DbContext factory (migration oluşturmak için)
    /// </summary>
    public class InvoiceDbContextFactory : IDesignTimeDbContextFactory<InvoiceDbContext>
    {
        public InvoiceDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<InvoiceDbContext>();
            
            // Kendi veritabanımız - LocalDB (migration için)
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=InvoiceTransferDb;Trusted_Connection=True;TrustServerCertificate=True;");
            
            return new InvoiceDbContext(optionsBuilder.Options);
        }
    }
}
