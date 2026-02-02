using System;

namespace InvoiceTransferApp.Core.ViewModel
{
    /// <summary>
    /// Panel / oturum kullanıcı bilgisi (Session ile kullanım için)
    /// </summary>
    public class PanelUserDto
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public DateTime LoginTime { get; set; }
    }
}
