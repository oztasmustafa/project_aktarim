using System;
using System.Collections.Generic;
using System.Data;
using InvoiceTransferApp.Core.Entities;
using InvoiceTransferApp.Core.Interfaces;

namespace InvoiceTransferApp.Repository.Repositories
{
    /// <summary>
    /// Netsis sistemi entegrasyonu - direkt SQL/stored procedure çağrıları
    /// </summary>
    public class NetsisRepository : INetsisRepository
    {
        private readonly string _connectionString;
        private List<Invoice> _lastFetchedInvoices;

        // Ekip arkadaşından: fatura listesi (TBLFATUIRS). 
        // NOT: FT.TARIH fatura tarihi, FT.ODEMETARIHI vade tarihi.
        //      WHERE'de FT.TARIH ile filtreleme yapılır.
        private const string SalesInvoicesSql = @"
SELECT
    FT.SUBE_KODU,
    FT.FATIRS_NO,
    FT.CARI_KODU,
    DBO.TRK(CS.CARI_ISIM) CARIISIM,
    FT.CARI_KOD2,
    DBO.TRK(FT.ACIKLAMA) ACIKLAMA,
    FT.KOD1,
    FT.ODEMEGUNU,
    FT.TARIH,
    FT.ODEMETARIHI,
    FT.PLA_KODU,
    DBO.TRK(P.PLASIYER_ACIKLAMA) PLASIYERACIKLAMA,
    FT.GIB_FATIRS_NO,
    FT.KDV_DAHILMI,
    FT.FATKALEM_ADEDI,
    FT.BRUTTUTAR,
    FT.SAT_ISKT,
    FT.MFAZ_ISKT,
    FT.GEN_ISK1T,
    FT.GEN_ISK1O,
    FT.GEN_ISK2T,
    FT.GEN_ISK2O,
    FT.GEN_ISK3T,
    FT.GEN_ISK3O,
    FT.KDV,
    FT.FAT_ALTM1,
    FT.FAT_ALTM2,
    FT.FAT_ALTM3,
    FT.GENELTOPLAM,
    DBO.TRK(EK.ACIK1) ACIK1,
    DBO.TRK(EK.ACIK2) ACIK2,
    DBO.TRK(EK.ACIK3) ACIK3,
    DBO.TRK(EK.ACIK4) ACIK4,
    DBO.TRK(EK.ACIK5) ACIK5,
    DBO.TRK(EK.ACIK6) ACIK6,
    DBO.TRK(EK.ACIK7) ACIK7,
    DBO.TRK(EK.ACIK8) ACIK8,
    DBO.TRK(EK.ACIK9) ACIK9,
    DBO.TRK(EK.ACIK10) ACIK10,
    DBO.TRK(EK.ACIK11) ACIK11,
    DBO.TRK(EK.ACIK12) ACIK12,
    DBO.TRK(EK.ACIK13) ACIK13,
    DBO.TRK(EK.ACIK14) ACIK14,
    DBO.TRK(EK.ACIK15) ACIK15,
    DBO.TRK(EK.ACIK16) ACIK16
FROM TBLFATUIRS FT
INNER JOIN TBLCASABIT CS ON CS.CARI_KOD = FT.CARI_KODU
INNER JOIN TBLCARIPLASIYER P ON P.PLASIYER_KODU = FT.PLA_KODU
LEFT JOIN TBLFATUEK EK ON EK.FKOD = FT.FTIRSIP AND FT.SUBE_KODU = EK.SUBE_KODU AND FT.FATIRS_NO = EK.FATIRSNO AND FT.CARI_KODU = EK.CKOD
WHERE FT.FTIRSIP = '1'
    AND CONVERT(DATE, FT.TARIH) >= CONVERT(DATE, @startDate)
    AND CONVERT(DATE, FT.TARIH) <= CONVERT(DATE, @endDate)";

        // Ekip arkadaşından: popup fatura kalemleri (TBLSTHAR). FISNO parametre ile verilir.
        private const string InvoiceItemsSql = @"
SELECT
    STH.STOK_KODU,
    DBO.TRK(ST.STOK_ADI) STOKADI,
    STH.STHAR_GCMIK MIKTAR,
    STH.STHAR_GCMIK2,
    STH.STHAR_TARIH,
    STH.STHAR_NF,
    STH.STHAR_BF,
    STH.STHAR_KDV,
    STH.DEPO_KODU,
    STH.STHAR_ACIKLAMA,
    STH.STHAR_SATISK,
    STH.STHAR_MALFISK,
    STH.STHAR_SATISK2,
    STH.STRA_SATISK3,
    STH.STRA_SATISK4,
    STH.STRA_SATISK5,
    STH.STRA_SATISK6,
    STH.STHAR_ODEGUN,
    STH.STHAR_KOD1,
    STH.EKALAN_NEDEN,
    STH.EKALAN,
    STH.SIRA,
    STH.STHAR_TESTAR,
    STH.SUBE_KODU
FROM TBLSTHAR STH
INNER JOIN TBLSTSABIT ST ON ST.STOK_KODU = STH.STOK_KODU
WHERE STH.FISNO = @fisno AND STH.STHAR_FTIRSIP = '1'";

        public NetsisRepository()
        {
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["NetsisDB"]?.ConnectionString ?? "";
        }

        public NetsisRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Invoice> GetSalesInvoices(string sourceCompany, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Netsis bağlantı dizesi tanımlı değil (NetsisDB).");

            var sql = SalesInvoicesSql;
            // SUBE_KODU bazi veritabanlarinda smallint; metin (ornegin "Erzurum_2026 Satis Fat") gecince filtre ekleme
            var filterBySube = !string.IsNullOrWhiteSpace(sourceCompany) && int.TryParse(sourceCompany.Trim(), out _);
            if (filterBySube)
                sql += " AND FT.SUBE_KODU = @sourceCompany";

            // DEBUG LOG
            var logPath = @"C:\Temp\NetsisRepositoryDebug.txt";
            try
            {
                var logMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] GetSalesInvoices called\n" +
                             $"  sourceCompany: '{sourceCompany}'\n" +
                             $"  startDate: {startDate:yyyy-MM-dd} (Date: {startDate.Date:yyyy-MM-dd})\n" +
                             $"  endDate: {endDate:yyyy-MM-dd} (Date: {endDate.Date:yyyy-MM-dd})\n" +
                             $"  filterBySube: {filterBySube}\n" +
                             $"  SQL Length: {sql.Length}\n";
                System.IO.File.AppendAllText(logPath, logMsg);
            }
            catch { /* Log hatası önemli değil */ }

            var list = new List<Invoice>();
            using (var conn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                {
                    // DateTime olarak gönder (Date değil) - SQL Server DATETIME sütunuyla uyumlu
                    cmd.Parameters.AddWithValue("@startDate", startDate.Date);
                    cmd.Parameters.AddWithValue("@endDate", endDate.Date);
                    
                    if (filterBySube)
                        cmd.Parameters.AddWithValue("@sourceCompany", sourceCompany.Trim());

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var inv = MapRowToInvoice(reader);
                            list.Add(inv);
                        }
                    }
                }
            }

            // DEBUG LOG - Result
            try
            {
                var logMsg = $"  Result: {list.Count} invoices fetched\n" +
                             (list.Count > 0 ? $"  First Invoice Date: {list[0].InvoiceDate:yyyy-MM-dd}\n" : "") +
                             (list.Count > 1 ? $"  Last Invoice Date: {list[list.Count - 1].InvoiceDate:yyyy-MM-dd}\n" : "") +
                             "---\n";
                System.IO.File.AppendAllText(logPath, logMsg);
            }
            catch { /* Log hatası önemli değil */ }

            _lastFetchedInvoices = list;
            return list;
        }

        public Invoice GetInvoiceById(int id)
        {
            if (_lastFetchedInvoices == null)
                return null;
            var inv = _lastFetchedInvoices.Find(x => x.Id == id);
            if (inv != null && !string.IsNullOrEmpty(_connectionString))
            {
                try
                {
                    inv.Items = GetInvoiceItems(inv.InvoiceNumber, inv.CompanyCode);
                }
                catch
                {
                    inv.Items = inv.Items ?? new List<InvoiceItem>();
                }
            }
            return inv;
        }

        /// <summary>Fatura kalemlerini Netsis TBLSTHAR'dan getirir (popup için).</summary>
        private List<InvoiceItem> GetInvoiceItems(string fisno, string subeKodu)
        {
            var list = new List<InvoiceItem>();
            if (string.IsNullOrWhiteSpace(fisno)) return list;
            var sql = InvoiceItemsSql;
            // SUBE_KODU smallint ise sadece sayisal degerde filtrele
            var filterBySube = !string.IsNullOrWhiteSpace(subeKodu) && int.TryParse(subeKodu.Trim(), out _);
            if (filterBySube)
                sql += " AND STH.SUBE_KODU = @subeKodu";
            using (var conn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@fisno", fisno.Trim());
                    if (filterBySube)
                        cmd.Parameters.AddWithValue("@subeKodu", subeKodu.Trim());
                    using (var reader = cmd.ExecuteReader())
                    {
                        int idx = 0;
                        while (reader.Read())
                        {
                            idx++;
                            list.Add(MapRowToInvoiceItem(reader, idx));
                        }
                    }
                }
            }
            return list;
        }

        private static InvoiceItem MapRowToInvoiceItem(IDataRecord reader, int id)
        {
            var qty = GetDecimal(reader, "MIKTAR");
            var unitPrice = GetDecimal(reader, "STHAR_NF");
            if (unitPrice == 0)
                unitPrice = GetDecimal(reader, "STHAR_BF");
            var lineTotal = qty * unitPrice;
            var taxAmount = GetDecimal(reader, "STHAR_KDV");
            var taxRate = lineTotal != 0 ? (taxAmount / lineTotal) * 100 : 0;
            return new InvoiceItem
            {
                Id = id,
                InvoiceId = 0,
                ProductCode = GetString(reader, "STOK_KODU"),
                ProductName = GetString(reader, "STOKADI"),
                Quantity = qty,
                UnitPrice = unitPrice,
                LineTotal = lineTotal,
                TaxRate = taxRate,
                TaxAmount = taxAmount,
                WarehouseCode = GetString(reader, "DEPO_KODU"),
                Description = GetString(reader, "STHAR_ACIKLAMA")
            };
        }

        /// <summary>Faturayı hedef şirkete Netsis'e yazar (TBLFATUIRS + TBLSTHAR).</summary>
        public string SaveInvoice(Fatura fatura, string targetCompanyCode)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Netsis bağlantı dizesi tanımlı değil (NetsisDB).");
            if (fatura?.FatUst == null)
                throw new ArgumentNullException(nameof(fatura), "Fatura başlığı boş olamaz.");
            if (fatura.Kalems == null || fatura.Kalems.Count == 0)
                throw new ArgumentException("Fatura kalemleri boş olamaz.", nameof(fatura));

            int subeKodu = 0;
            if (!string.IsNullOrWhiteSpace(targetCompanyCode) && int.TryParse(targetCompanyCode.Trim(), out int parsed))
                subeKodu = parsed;

            using (var conn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var u = fatura.FatUst;
                        // TBLFATUIRS: FTIRSIP='2' alış faturası, hedef şirkete yazılıyor
                        const string insertUst = @"
INSERT INTO TBLFATUIRS (
    FTIRSIP, SUBE_KODU, FATIRS_NO, CARI_KODU, TARIH, ODEMETARIHI, ACIKLAMA,
    TIPI, KDV_DAHILMI, PLA_KODU, BRUTTUTAR, KDV, GENELTOPLAM, FATKALEM_ADEDI
) VALUES (
    '2', @SubeKodu, @FATIRS_NO, @CariKod, @Tarih, @ODEMETARIHI, @Aciklama,
    @TIPI, @KDV_DAHILMI, @PLA_KODU, @BRUTTUTAR, @KDV, @GENELTOPLAM, @FATKALEM_ADEDI
)";
                        using (var cmd = new System.Data.SqlClient.SqlCommand(insertUst, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@SubeKodu", subeKodu);
                            cmd.Parameters.AddWithValue("@FATIRS_NO", (object)u.FATIRS_NO ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CariKod", (object)u.CariKod ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Tarih", u.Tarih);
                            cmd.Parameters.AddWithValue("@ODEMETARIHI", u.ODEMETARIHI);
                            cmd.Parameters.AddWithValue("@Aciklama", (object)u.Aciklama ?? "");
                            cmd.Parameters.AddWithValue("@TIPI", u.TIPI);
                            cmd.Parameters.AddWithValue("@KDV_DAHILMI", u.KDV_DAHILMI);
                            cmd.Parameters.AddWithValue("@PLA_KODU", (object)u.PLA_KODU ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@BRUTTUTAR", u.FAT_ALTM2);
                            cmd.Parameters.AddWithValue("@KDV", (decimal)0);
                            cmd.Parameters.AddWithValue("@GENELTOPLAM", (decimal)(u.FAT_ALTM2));
                            cmd.Parameters.AddWithValue("@FATKALEM_ADEDI", fatura.Kalems.Count);
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var k in fatura.Kalems)
                        {
                            decimal birimFiyatVal = 0;
                            decimal.TryParse(k.STra_NF?.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out birimFiyatVal);
                            const string insertKalem = @"
INSERT INTO TBLSTHAR (
    STHAR_FTIRSIP, SUBE_KODU, FISNO, STOK_KODU, STHAR_GCMIK, STHAR_NF, STHAR_KDV,
    DEPO_KODU, SIRA, STHAR_ACIKLAMA
) VALUES (
    '2', @SubeKodu, @FISNO, @StokKodu, @Miktar, @BirimFiyat, @Kdv,
    @DepoKodu, @Sira, @Aciklama
)";
                            using (var cmd = new System.Data.SqlClient.SqlCommand(insertKalem, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@SubeKodu", subeKodu);
                                cmd.Parameters.AddWithValue("@FISNO", (object)u.FATIRS_NO ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@StokKodu", (object)k.StokKodu ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Miktar", k.STra_GCMIK);
                                cmd.Parameters.AddWithValue("@BirimFiyat", birimFiyatVal);
                                cmd.Parameters.AddWithValue("@Kdv", (double)k.STra_KDV);
                                cmd.Parameters.AddWithValue("@DepoKodu", k.DEPO_KODU);
                                cmd.Parameters.AddWithValue("@Sira", k.Sira);
                                cmd.Parameters.AddWithValue("@Aciklama", (object)k.STra_ACIK ?? "");
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                        return u.FATIRS_NO ?? "";
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw new InvalidOperationException($"Netsis yazma hatası: {ex.Message}", ex);
                    }
                }
            }
        }

        private static Invoice MapRowToInvoice(IDataRecord reader)
        {
            // Benzersiz ID oluştur: FATIRS_NO + SUBE_KODU kombinasyonundan hash
            var fatNo = GetString(reader, "FATIRS_NO") ?? "";
            var subeKodu = GetString(reader, "SUBE_KODU") ?? "";
            var uniqueKey = $"{subeKodu}_{fatNo}";
            var id = Math.Abs(uniqueKey.GetHashCode());
            
            // FT.TARIH: fatura tarihi, FT.ODEMETARIHI: vade tarihi
            var faturaTarihi = GetDateTimeNull(reader, "TARIH");
            var vadeTarihi = GetDateTimeNull(reader, "ODEMETARIHI");
            
            var inv = new Invoice
            {
                Id = id,
                InvoiceNumber = fatNo,
                CustomerCode = GetString(reader, "CARI_KODU"),
                CustomerName = GetString(reader, "CARIISIM"),
                Description = GetString(reader, "ACIKLAMA"),
                CompanyCode = GetString(reader, "SUBE_KODU"),
                InvoiceDate = faturaTarihi ?? DateTime.Today,
                DueDate = vadeTarihi,
                TotalAmount = GetDecimal(reader, "GENELTOPLAM"),
                TaxAmount = GetDecimal(reader, "KDV"),
                NetAmount = GetDecimal(reader, "BRUTTUTAR"),
                Currency = "TRY",
                Status = "Beklemede",
                InvoiceType = "Satış",
                CreatedDate = DateTime.Now,
                IsTransferredToNetsis = false,
                Items = new List<InvoiceItem>(),
                CariKod2 = GetString(reader, "CARI_KOD2"),
                Kod1 = GetString(reader, "KOD1"),
                OdemeGunu = GetIntNull(reader, "ODEMEGUNU"),
                PlaKodu = GetString(reader, "PLA_KODU"),
                PlasiyerAciklama = GetString(reader, "PLASIYERACIKLAMA"),
                GibFatirsNo = GetString(reader, "GIB_FATIRS_NO"),
                KdvDahilmi = GetBoolNull(reader, "KDV_DAHILMI"),
                FatKalemAdedi = GetIntNull(reader, "FATKALEM_ADEDI"),
                Bruttutar = GetDecimal(reader, "BRUTTUTAR"),
                SatIskt = GetDecimal(reader, "SAT_ISKT"),
                MfazIskt = GetDecimal(reader, "MFAZ_ISKT"),
                GenIsk1T = GetDecimal(reader, "GEN_ISK1T"),
                GenIsk1O = GetDecimal(reader, "GEN_ISK1O"),
                GenIsk2T = GetDecimal(reader, "GEN_ISK2T"),
                GenIsk2O = GetDecimal(reader, "GEN_ISK2O"),
                GenIsk3T = GetDecimal(reader, "GEN_ISK3T"),
                GenIsk3O = GetDecimal(reader, "GEN_ISK3O"),
                FatAltm1 = GetString(reader, "FAT_ALTM1"),
                FatAltm2 = GetString(reader, "FAT_ALTM2"),
                FatAltm3 = GetString(reader, "FAT_ALTM3"),
                GenelToplam = GetDecimal(reader, "GENELTOPLAM"),
                Acik1 = GetString(reader, "ACIK1"),
                Acik2 = GetString(reader, "ACIK2"),
                Acik3 = GetString(reader, "ACIK3"),
                Acik4 = GetString(reader, "ACIK4"),
                Acik5 = GetString(reader, "ACIK5"),
                Acik6 = GetString(reader, "ACIK6"),
                Acik7 = GetString(reader, "ACIK7"),
                Acik8 = GetString(reader, "ACIK8"),
                Acik9 = GetString(reader, "ACIK9"),
                Acik10 = GetString(reader, "ACIK10"),
                Acik11 = GetString(reader, "ACIK11"),
                Acik12 = GetString(reader, "ACIK12"),
                Acik13 = GetString(reader, "ACIK13"),
                Acik14 = GetString(reader, "ACIK14"),
                Acik15 = GetString(reader, "ACIK15"),
                Acik16 = GetString(reader, "ACIK16")
            };

            return inv;
        }

        private static string GetString(IDataRecord reader, string colName)
        {
            try
            {
                var i = reader.GetOrdinal(colName);
                if (reader.IsDBNull(i)) return null;
                var value = reader.GetValue(i);
                return value == null ? null : Convert.ToString(value);
            }
            catch { return null; }
        }

        private static decimal GetDecimal(IDataRecord reader, string colName)
        {
            try
            {
                var i = reader.GetOrdinal(colName);
                return reader.IsDBNull(i) ? 0 : Convert.ToDecimal(reader.GetValue(i));
            }
            catch { return 0; }
        }

        private static int? GetIntNull(IDataRecord reader, string colName)
        {
            try
            {
                var i = reader.GetOrdinal(colName);
                return reader.IsDBNull(i) ? (int?)null : Convert.ToInt32(reader.GetValue(i));
            }
            catch { return null; }
        }

        private static bool? GetBoolNull(IDataRecord reader, string colName)
        {
            try
            {
                var i = reader.GetOrdinal(colName);
                if (reader.IsDBNull(i)) return null;
                var v = reader.GetValue(i);
                if (v is bool b) return b;
                if (v is int n) return n != 0;
                if (v is byte by) return by != 0;   // SQL Server BIT genelde byte döner
                if (v is short s) return s != 0;
                if (v is long l) return l != 0;
                if (v is decimal d) return d != 0;
                return null;
            }
            catch { return null; }
        }

        private static DateTime? GetDateTimeNull(IDataRecord reader, string colName)
        {
            try
            {
                var i = reader.GetOrdinal(colName);
                return reader.IsDBNull(i) ? (DateTime?)null : reader.GetDateTime(i);
            }
            catch { return null; }
        }
    }
}
