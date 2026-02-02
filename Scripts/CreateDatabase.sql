-- InvoiceTransferDb: Kendi veritabanımız (Fatura Aktarım)
-- SQL Server / SQL Server Express üzerinde çalıştırın (SSMS veya sqlcmd)

USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InvoiceTransferDb')
BEGIN
    CREATE DATABASE InvoiceTransferDb;
END
GO

USE InvoiceTransferDb;
GO

-- Invoices (Fatura başlıkları)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
BEGIN
    CREATE TABLE Invoices (
        Id                   INT IDENTITY(1,1) NOT NULL,
        InvoiceNumber         NVARCHAR(50)     NOT NULL,
        InvoiceDate           DATETIME2         NOT NULL,
        CustomerCode          NVARCHAR(50)      NULL,
        CustomerName          NVARCHAR(200)     NULL,
        TotalAmount           DECIMAL(18,2)     NOT NULL,
        Currency              NVARCHAR(10)      NULL,
        Description           NVARCHAR(MAX)     NULL,
        Status                NVARCHAR(50)      NULL,
        TaxAmount             DECIMAL(18,2)     NOT NULL DEFAULT 0,
        TaxRate               DECIMAL(18,2)     NOT NULL DEFAULT 0,
        NetAmount             DECIMAL(18,2)     NOT NULL DEFAULT 0,
        DueDate               DATETIME2         NULL,
        CompanyCode           NVARCHAR(50)      NULL,
        InvoiceType           NVARCHAR(50)      NULL,
        CreatedDate           DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy             NVARCHAR(100)     NULL,
        UpdatedDate            DATETIME2         NULL,
        UpdatedBy             NVARCHAR(100)     NULL,
        IsTransferredToNetsis BIT              NOT NULL DEFAULT 0,
        TransferDate          DATETIME2         NULL,
        NetsisReferenceNumber NVARCHAR(100)     NULL,
        Notes                 NVARCHAR(MAX)     NULL,
        CONSTRAINT PK_Invoices PRIMARY KEY (Id)
    );
END
GO

-- InvoiceItems (Fatura kalemleri)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InvoiceItems')
BEGIN
    CREATE TABLE InvoiceItems (
        Id           INT IDENTITY(1,1) NOT NULL,
        InvoiceId    INT               NOT NULL,
        ProductCode  NVARCHAR(50)      NULL,
        ProductName  NVARCHAR(200)     NULL,
        Quantity     DECIMAL(18,2)     NOT NULL,
        UnitPrice    DECIMAL(18,2)     NOT NULL,
        LineTotal    DECIMAL(18,2)     NOT NULL,
        TaxRate      DECIMAL(18,2)     NOT NULL DEFAULT 0,
        TaxAmount    DECIMAL(18,2)    NOT NULL DEFAULT 0,
        WarehouseCode NVARCHAR(50)     NULL,
        Description   NVARCHAR(MAX)    NULL,
        CONSTRAINT PK_InvoiceItems PRIMARY KEY (Id),
        CONSTRAINT FK_InvoiceItems_Invoices_InvoiceId
            FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_InvoiceItems_InvoiceId ON InvoiceItems(InvoiceId);
END
GO

-- ErrorLogs (Hata / uygulama logları)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ErrorLogs')
BEGIN
    CREATE TABLE ErrorLogs (
        Id          INT IDENTITY(1,1) NOT NULL,
        LogDate     DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
        Level       NVARCHAR(20)      NOT NULL,  -- Error, Warning, Info
        Message     NVARCHAR(500)     NOT NULL,
        Exception   NVARCHAR(MAX)     NULL,       -- Exception.ToString()
        Source      NVARCHAR(200)     NULL,       -- Sınıf / metod adı
        UserName    NVARCHAR(100)     NULL,
        MachineName NVARCHAR(100)     NULL,
        Extra       NVARCHAR(MAX)     NULL,
        CONSTRAINT PK_ErrorLogs PRIMARY KEY (Id)
    );
    CREATE INDEX IX_ErrorLogs_LogDate ON ErrorLogs(LogDate);
    CREATE INDEX IX_ErrorLogs_Level   ON ErrorLogs(Level);
END
GO

-- AppSettings (uygulama ayarları: kolon görünürlüğü vb.)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AppSettings')
BEGIN
    CREATE TABLE AppSettings (
        [Key]   NVARCHAR(100)  NOT NULL,
        [Value] NVARCHAR(MAX)  NULL,
        CONSTRAINT PK_AppSettings PRIMARY KEY ([Key])
    );
END
GO

PRINT 'InvoiceTransferDb veritabani ve tablolar hazir.';
GO
