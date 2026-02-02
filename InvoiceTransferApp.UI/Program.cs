using System;
using System.Windows.Forms;

namespace InvoiceTransferApp.UI
{
    /// <summary>
    /// Uygulama giriş noktası
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Uygulamanın ana giriş noktası
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Windows Forms uygulama ayarları
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ana formu başlat
            Application.Run(new MainForm());
        }
    }
}
