using System;

namespace InvoiceTransferApp.Service.Helper
{
    /// <summary>
    /// Tarih yardımcı sınıfı (örnek yapıdaki Helper klasörüne uyum)
    /// </summary>
    public static class DateHelper
    {
        public static DateTime TruncateToDate(DateTime value)
        {
            return value.Date;
        }

        public static bool IsDateInRange(DateTime date, DateTime start, DateTime end)
        {
            return date >= start.Date && date <= end.Date;
        }
    }
}
