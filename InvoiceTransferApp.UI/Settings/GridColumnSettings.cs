using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace InvoiceTransferApp.UI.Settings
{
    /// <summary>
    /// Grid kolon tanımları ve görünür kolon tercihi (SQL AppSettings tablosunda saklanır).
    /// </summary>
    public static class GridColumnSettings
    {
        private const string AppSettingsKey = "VisibleGridColumns";

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["InvoiceTransferDb"]?.ConnectionString ?? "";
        }

        /// <summary>
        /// Tüm grid kolonları: SELECT sorgusundaki sıraya göre düzenlenmiş
        /// FieldName (ViewModel property) ve Caption (Türkçe başlık)
        /// </summary>
        public static List<GridColumnDefinition> GetAllDefinitions()
        {
            return new List<GridColumnDefinition>
            {
                // İlk kolon: Fatura Tarihi (kullanıcı isteği)
                new GridColumnDefinition("InvoiceDate", "Fatura Tarihi"),          // FT.TARIH
                
                // SELECT sırasına göre (ekip arkadaşının sorgusu)
                new GridColumnDefinition("CompanyCode", "Şube Kodu"),              // FT.SUBE_KODU
                new GridColumnDefinition("InvoiceNumber", "Fatura No"),            // FT.FATIRS_NO
                new GridColumnDefinition("CustomerCode", "Cari Kod"),              // FT.CARI_KODU
                new GridColumnDefinition("CustomerName", "Cari Adı"),              // CARIISIM
                new GridColumnDefinition("CariKod2", "Cari Kod 2"),                // FT.CARI_KOD2
                new GridColumnDefinition("Description", "Açıklama"),               // ACIKLAMA
                new GridColumnDefinition("Kod1", "Kod 1"),                         // FT.KOD1
                new GridColumnDefinition("OdemeGunu", "Ödeme Günü"),               // FT.ODEMEGUNU
                new GridColumnDefinition("DueDate", "Vade Tarihi"),                // FT.ODEMETARIHI
                new GridColumnDefinition("PlaKodu", "Plasiyer Kodu"),              // FT.PLA_KODU
                new GridColumnDefinition("PlasiyerAciklama", "Plasiyer"),          // PLASIYERACIKLAMA
                new GridColumnDefinition("GibFatirsNo", "GIB Fatura No"),          // FT.GIB_FATIRS_NO
                new GridColumnDefinition("KdvDahilmi", "KDV Dahil mi"),            // FT.KDV_DAHILMI
                new GridColumnDefinition("FatKalemAdedi", "Kalem Adedi"),          // FT.FATKALEM_ADEDI
                new GridColumnDefinition("Bruttutar", "Brüt Tutar"),               // FT.BRUTTUTAR
                new GridColumnDefinition("SatIskt", "Satış İsk. T"),               // FT.SAT_ISKT
                new GridColumnDefinition("MfazIskt", "Mfaz İsk. T"),               // FT.MFAZ_ISKT
                new GridColumnDefinition("GenIsk1T", "Gen İsk. 1 T"),              // FT.GEN_ISK1T
                new GridColumnDefinition("GenIsk1O", "Gen İsk. 1 O"),              // FT.GEN_ISK1O
                new GridColumnDefinition("GenIsk2T", "Gen İsk. 2 T"),              // FT.GEN_ISK2T
                new GridColumnDefinition("GenIsk2O", "Gen İsk. 2 O"),              // FT.GEN_ISK2O
                new GridColumnDefinition("GenIsk3T", "Gen İsk. 3 T"),              // FT.GEN_ISK3T
                new GridColumnDefinition("GenIsk3O", "Gen İsk. 3 O"),              // FT.GEN_ISK3O
                new GridColumnDefinition("TaxAmount", "KDV"),                      // FT.KDV
                new GridColumnDefinition("FatAltm1", "Fat Alt M1"),                // FT.FAT_ALTM1
                new GridColumnDefinition("FatAltm2", "Fat Alt M2"),                // FT.FAT_ALTM2
                new GridColumnDefinition("FatAltm3", "Fat Alt M3"),                // FT.FAT_ALTM3
                new GridColumnDefinition("GenelToplam", "Genel Toplam"),           // FT.GENELTOPLAM
                new GridColumnDefinition("Acik1", "Açık 1"),                       // ACIK1
                new GridColumnDefinition("Acik2", "Açık 2"),                       // ACIK2
                new GridColumnDefinition("Acik3", "Açık 3"),                       // ACIK3
                new GridColumnDefinition("Acik4", "Açık 4"),                       // ACIK4
                new GridColumnDefinition("Acik5", "Açık 5"),                       // ACIK5
                new GridColumnDefinition("Acik6", "Açık 6"),                       // ACIK6
                new GridColumnDefinition("Acik7", "Açık 7"),                       // ACIK7
                new GridColumnDefinition("Acik8", "Açık 8"),                       // ACIK8
                new GridColumnDefinition("Acik9", "Açık 9"),                       // ACIK9
                new GridColumnDefinition("Acik10", "Açık 10"),                     // ACIK10
                new GridColumnDefinition("Acik11", "Açık 11"),                     // ACIK11
                new GridColumnDefinition("Acik12", "Açık 12"),                     // ACIK12
                new GridColumnDefinition("Acik13", "Açık 13"),                     // ACIK13
                new GridColumnDefinition("Acik14", "Açık 14"),                     // ACIK14
                new GridColumnDefinition("Acik15", "Açık 15"),                     // ACIK15
                new GridColumnDefinition("Acik16", "Açık 16"),                     // ACIK16
                
                // Uygulama için ek kolonlar (SELECT'te yok)
                new GridColumnDefinition("IsTransferredToNetsis", "Aktarım Durumu"),
                new GridColumnDefinition("NetsisReferenceNumber", "Netsis Ref. No")
            };
        }

        /// <summary>
        /// Görünür kolonların FieldName listesi (SQL AppSettings'ten). Boş/null = tümü görünsün.
        /// </summary>
        public static List<string> GetVisibleFieldNames()
        {
            var connStr = GetConnectionString();
            if (string.IsNullOrEmpty(connStr)) return null;
            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT [Value] FROM AppSettings WHERE [Key] = @Key", conn))
                    {
                        cmd.Parameters.AddWithValue("@Key", AppSettingsKey);
                        var value = cmd.ExecuteScalar() as string;
                        if (string.IsNullOrWhiteSpace(value)) return null;
                        
                        var result = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .Where(s => s.Length > 0)
                            .ToList();
                        
                        // Debug: Okunan değeri göster
                        System.Diagnostics.Debug.WriteLine($"GridColumnSettings.GetVisibleFieldNames(): {value}");
                        System.Diagnostics.Debug.WriteLine($"Parsed count: {result.Count}");
                        
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GridColumnSettings.GetVisibleFieldNames() HATA: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Görünür kolon listesini SQL AppSettings tablosuna kaydeder.
        /// </summary>
        public static void SetVisibleFieldNames(List<string> fieldNames)
        {
            var connStr = GetConnectionString();
            if (string.IsNullOrEmpty(connStr))
            {
                System.Windows.Forms.MessageBox.Show("Bağlantı dizesi bulunamadı!", "Hata", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }
            
            try
            {
                var value = (fieldNames != null && fieldNames.Count > 0)
                    ? string.Join(",", fieldNames)
                    : "";
                    
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        IF EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = @Key)
                            UPDATE AppSettings SET [Value] = @Value WHERE [Key] = @Key;
                        ELSE
                            INSERT INTO AppSettings ([Key], [Value]) VALUES (@Key, @Value);", conn))
                    {
                        cmd.Parameters.AddWithValue("@Key", AppSettingsKey);
                        cmd.Parameters.AddWithValue("@Value", value);
                        cmd.ExecuteNonQuery();
                    }
                }
                
                System.Windows.Forms.MessageBox.Show("Kolon ayarları kaydedildi.", "Bilgi", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Kolon ayarları kaydedilemedi!\nHata: {ex.Message}", "Hata", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }

    public class GridColumnDefinition
    {
        public string FieldName { get; }
        public string Caption { get; }
        public GridColumnDefinition(string fieldName, string caption)
        {
            FieldName = fieldName;
            Caption = caption;
        }
    }
}
