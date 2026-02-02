using System;
using System.IO;
using Microsoft.Data.SqlClient;

// InvoiceTransferDb veritabanını LocalDB'de oluşturur (gerçek kurulum).
// Çalıştırma: dotnet run (Scripts/DbSetup klasöründen) veya dotnet run --project Scripts/DbSetup
class Program
{
    static void Main()
    {
        const string connMaster = "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
        Console.WriteLine("LocalDB'de InvoiceTransferDb kuruluyor...");

        try
        {
            using (var conn = new SqlConnection(connMaster))
            {
                conn.Open();

                // 1) Veritabanı
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InvoiceTransferDb')
BEGIN
    CREATE DATABASE InvoiceTransferDb;
    PRINT 'InvoiceTransferDb olusturuldu.';
END
ELSE
    PRINT 'InvoiceTransferDb zaten var.';
";
                    cmd.ExecuteNonQuery();
                }
            }

            // 2) Tablolar (InvoiceTransferDb bağlantısı ile)
            var connDb = "Server=(localdb)\\mssqllocaldb;Database=InvoiceTransferDb;Trusted_Connection=True;TrustServerCertificate=True;";
            using (var conn = new SqlConnection(connDb))
            {
                conn.Open();

                var scriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "CreateDatabase.sql");
                if (!File.Exists(scriptPath))
                    scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "CreateDatabase.sql");
                if (!File.Exists(scriptPath))
                    scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "CreateDatabase.sql");

                string sql;
                if (File.Exists(scriptPath))
                {
                    sql = File.ReadAllText(scriptPath);
                    sql = sql.Replace("USE master;", "").Replace("USE InvoiceTransferDb;", "");
                }
                else
                    sql = GetEmbeddedSql();

                var batches = sql.Split(new[] { "\r\nGO\r\n", "\nGO\n", "GO" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var batch in batches)
                {
                    var b = batch.Trim();
                    if (string.IsNullOrWhiteSpace(b) || b.StartsWith("--") || b.StartsWith("PRINT "))
                        continue;
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = b;
                            cmd.CommandTimeout = 30;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("already exists"))
                            Console.WriteLine("Uyari: " + ex.Message);
                    }
                }
            }

            Console.WriteLine("Kurulum tamamlandi. InvoiceTransferDb ve tablolar hazir.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("HATA: " + ex.Message);
            Environment.Exit(1);
        }
    }

    static string GetEmbeddedSql()
    {
        return @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
BEGIN
    CREATE TABLE Invoices (
        Id INT IDENTITY(1,1) NOT NULL,
        InvoiceNumber NVARCHAR(50) NOT NULL,
        InvoiceDate DATETIME2 NOT NULL,
        CustomerCode NVARCHAR(50) NULL,
        CustomerName NVARCHAR(200) NULL,
        TotalAmount DECIMAL(18,2) NOT NULL,
        Currency NVARCHAR(10) NULL,
        Description NVARCHAR(MAX) NULL,
        Status NVARCHAR(50) NULL,
        TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        TaxRate DECIMAL(18,2) NOT NULL DEFAULT 0,
        NetAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        DueDate DATETIME2 NULL,
        CompanyCode NVARCHAR(50) NULL,
        InvoiceType NVARCHAR(50) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100) NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedBy NVARCHAR(100) NULL,
        IsTransferredToNetsis BIT NOT NULL DEFAULT 0,
        TransferDate DATETIME2 NULL,
        NetsisReferenceNumber NVARCHAR(100) NULL,
        Notes NVARCHAR(MAX) NULL,
        CONSTRAINT PK_Invoices PRIMARY KEY (Id)
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InvoiceItems')
BEGIN
    CREATE TABLE InvoiceItems (
        Id INT IDENTITY(1,1) NOT NULL,
        InvoiceId INT NOT NULL,
        ProductCode NVARCHAR(50) NULL,
        ProductName NVARCHAR(200) NULL,
        Quantity DECIMAL(18,2) NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        LineTotal DECIMAL(18,2) NOT NULL,
        TaxRate DECIMAL(18,2) NOT NULL DEFAULT 0,
        TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        WarehouseCode NVARCHAR(50) NULL,
        Description NVARCHAR(MAX) NULL,
        CONSTRAINT PK_InvoiceItems PRIMARY KEY (Id),
        CONSTRAINT FK_InvoiceItems_Invoices_InvoiceId FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_InvoiceItems_InvoiceId ON InvoiceItems(InvoiceId);
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ErrorLogs')
BEGIN
    CREATE TABLE ErrorLogs (
        Id          INT IDENTITY(1,1) NOT NULL,
        LogDate     DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
        Level       NVARCHAR(20)      NOT NULL,
        Message     NVARCHAR(500)     NOT NULL,
        Exception   NVARCHAR(MAX)     NULL,
        Source      NVARCHAR(200)     NULL,
        UserName    NVARCHAR(100)     NULL,
        MachineName NVARCHAR(100)     NULL,
        Extra       NVARCHAR(MAX)     NULL,
        CONSTRAINT PK_ErrorLogs PRIMARY KEY (Id)
    );
    CREATE INDEX IX_ErrorLogs_LogDate ON ErrorLogs(LogDate);
    CREATE INDEX IX_ErrorLogs_Level   ON ErrorLogs(Level);
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AppSettings')
BEGIN
    CREATE TABLE AppSettings (
        [Key]   NVARCHAR(100)  NOT NULL,
        [Value] NVARCHAR(MAX)  NULL,
        CONSTRAINT PK_AppSettings PRIMARY KEY ([Key])
    );
END
";
    }
}
