using Microsoft.EntityFrameworkCore;
using InvoiceTransferApp.Core.Entities;

namespace InvoiceTransferApp.Repository.Context
{
    /// <summary>
    /// EF Core DbContext - Invoice domain için
    /// Netsis entegrasyonunda isteğe bağlı (direkt SQL kullanılıyorsa)
    /// </summary>
    public class InvoiceDbContext : DbContext
    {
        public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InvoiceNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CustomerCode).HasMaxLength(50);
                entity.Property(e => e.CustomerName).HasMaxLength(200);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.HasMany(e => e.Items)
                    .WithOne()
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<InvoiceItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductCode).HasMaxLength(50);
                entity.Property(e => e.ProductName).HasMaxLength(200);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            });
        }
    }
}
