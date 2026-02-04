using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InvoiceTransferApp.UI.Forms;
using InvoiceTransferApp.UI.Settings;
using InvoiceTransferApp.Service.Services;
using InvoiceTransferApp.Service.Mapping;
using InvoiceTransferApp.Core.Entities;
using InvoiceTransferApp.Core.Interfaces;
using InvoiceTransferApp.Core.DTO;
using InvoiceTransferApp.Core.ViewModel;
using AutoMapper;

namespace InvoiceTransferApp.UI
{
    /// <summary>
    /// Fatura Aktarım Ana Formu
    /// Erzurum_2026 şirketinden Bakiye şirketine fatura aktarımı
    /// </summary>
    public partial class MainForm : Form
    {
        #region Fields

        private DateTimePicker dateStartDate;
        private DateTimePicker dateEndDate;
        private Button btnGetInvoices;
        private Button btnTransfer;
        private DataGridView gridInvoices;
        private Label lblStartDate;
        private Label lblEndDate;
        private Label lblTitle;
        private Label lblSearch;
        private TextBox txtSearch;
        private Label lblStatus;
        private ComboBox cmbStatus;
        private Button btnColumnSettings;
        private PictureBox picLogoFooter;
        private Label lblSoftwareBy;

        private List<InvoiceDto> _invoiceList;
        private List<Invoice> _transferredInvoices;
        private IInvoiceService _invoiceService;
        private IMapper _mapper;
        private PanelUserDto _panelUser;

        // Sabit şirket bilgileri
        private const string SOURCE_COMPANY = "Erzurum_2026 Satış Fat";
        private const string TARGET_COMPANY = "Bakiye Alış FAT";
        /// <summary></summary>
        private const string SOFTWARE_COMPANY_NAME = "S4IN A.Ş.";

        #endregion

        #region Constructor

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            
            // Servis katmanı fabrikası ile IInvoiceService alınır (UI, Repository'e referans vermez)
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<InvoiceTransferApp.Core.Mapping.MappingProfile>());
            _mapper = mapperConfig.CreateMapper();
            _invoiceService = InvoiceService.CreateDefault(_mapper);
            
            _panelUser = new PanelUserDto { UserName = Environment.UserName, DisplayName = Environment.UserName, LoginTime = DateTime.Now };

            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            
            // Form açılır açılmaz göster; veri arka planda yüklensin (uygulama geç açılmasın)
            this.Shown += (s, ev) => this.BeginInvoke(new Action(LoadInitialDataAsync));
            this.Load += MainForm_Load;
            this.Resize += MainForm_Resize;
        }

        private const int MarginBottom = 20;

        private void MainForm_Load(object sender, EventArgs e)
        {
            PositionButtonsAndGrid();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            PositionButtonsAndGrid();
        }

        /// <summary>
        /// Kolon Ayarları ve Aktar butonlarını tablonun (grid) sağ kenarıyla aynı hizada konumlar; grid yüksekliğini ayarlar.
        /// </summary>
        private void PositionButtonsAndGrid()
        {
            if (btnColumnSettings == null || btnTransfer == null || gridInvoices == null) return;
            int w = ClientSize.Width;
            int h = ClientSize.Height;
            if (w < 200 || h < 200) return;

            // Grid: sol 20px, sağda 80px boşluk (tablo sonu); butonlar tablonun sağ kenarıyla hizalı
            gridInvoices.Left = 20;
            gridInvoices.Width = w - 100;

            // Grid yüksekliği: Aktar butonunun üstünde boşluk bırak
            int gridBottom = h - btnTransfer.Height - MarginBottom - 10;
            if (gridBottom > 130)
            {
                gridInvoices.Height = gridBottom - 130;
            }

            // Kolon Ayarları: tablonun sağ üstüyle aynı hizada (grid.Right ile)
            btnColumnSettings.Left = gridInvoices.Right - btnColumnSettings.Width;
            btnColumnSettings.Top = 102;

            // Aktar: tablonun sağ altıyla aynı hizada
            btnTransfer.Left = gridInvoices.Right - btnTransfer.Width;
            btnTransfer.Top = h - btnTransfer.Height - MarginBottom;

            // Alt orta: logo + "Yazılım: Şirket" (logo yazının üstünde)
            if (lblSoftwareBy != null)
            {
                lblSoftwareBy.Top = h - lblSoftwareBy.Height - 6;
                lblSoftwareBy.Left = Math.Max(20, (w - lblSoftwareBy.Width) / 2);
            }
            if (picLogoFooter != null)
            {
                int footerLogoH = Math.Min(36, picLogoFooter.Height);
                picLogoFooter.Top = (lblSoftwareBy != null ? lblSoftwareBy.Top - footerLogoH - 4 : h - footerLogoH - 10);
                picLogoFooter.Left = Math.Max(20, (w - picLogoFooter.Width) / 2);
                picLogoFooter.BringToFront();
                if (lblSoftwareBy != null) lblSoftwareBy.BringToFront();
            }
        }

        #endregion

        #region Initialization Methods

        /// <summary>
        /// Form bileşenlerini başlatır ve düzenler
        /// </summary>
        private void InitializeCustomComponents()
        {
            this.Text = "Fatura Aktarım Uygulaması";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 600);

            _transferredInvoices = new List<Invoice>();
            _invoiceList = new List<InvoiceDto>();

            // Initialize single page UI
            InitializeComponents();
        }

        /// <summary>
        /// Ana UI bileşenlerini başlatır
        /// </summary>
        private void InitializeComponents()
        {
            // Başlık - sol köşede, Başlangıç Tarihi'nin üzerinde
            lblTitle = new Label
            {
                Text = "Şirketler Arası Fatura Aktarım Programı",
                Location = new Point(20, 10),
                Size = new Size(450, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            this.Controls.Add(lblTitle);

            // Start Date Label & DateTimePicker
            lblStartDate = new Label
            {
                Text = "Başlangıç Tarihi:",
                Location = new Point(20, 60),
                Size = new Size(100, 20)
            };
            this.Controls.Add(lblStartDate);

            dateStartDate = new DateTimePicker
            {
                Location = new Point(130, 57),
                Size = new Size(150, 20),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddMonths(-1)
            };
            this.Controls.Add(dateStartDate);

            // End Date Label & DateTimePicker
            lblEndDate = new Label
            {
                Text = "Bitiş Tarihi:",
                Location = new Point(310, 60),
                Size = new Size(80, 20)
            };
            this.Controls.Add(lblEndDate);

            dateEndDate = new DateTimePicker
            {
                Location = new Point(400, 57),
                Size = new Size(150, 20),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            this.Controls.Add(dateEndDate);

            // Get Invoices Button
            btnGetInvoices = new Button
            {
                Text = "Getir",
                Location = new Point(580, 55),
                Size = new Size(120, 26)
            };
            btnGetInvoices.Click += BtnGetInvoices_Click;
            this.Controls.Add(btnGetInvoices);

            // Status Label & ComboBox
            lblStatus = new Label
            {
                Text = "Durum:",
                Location = new Point(20, 95),
                Size = new Size(100, 20)
            };
            this.Controls.Add(lblStatus);

            cmbStatus = new ComboBox
            {
                Location = new Point(130, 92),
                Size = new Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new string[] { "Tümü", "Aktarılmayan", "Aktarılan" });
            cmbStatus.SelectedIndex = 0;
            this.Controls.Add(cmbStatus);

            // Search Label & TextBox
            lblSearch = new Label
            {
                Text = "Arama:",
                Location = new Point(310, 95),
                Size = new Size(80, 20)
            };
            this.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(400, 92),
                Size = new Size(300, 20),
                PlaceholderText = "Fatura no, cari kod, cari adı...",
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            this.Controls.Add(txtSearch);

            // DataGridView for Invoices - pencere büyüdüğünde alanı doldurur
            gridInvoices = new DataGridView
            {
                Location = new Point(20, 130),
                Size = new Size(1100, 390),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                RowHeadersVisible = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                RowHeadersWidth = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            
            gridInvoices.DoubleClick += GridViewInvoices_DoubleClick;
            gridInvoices.CellPainting += GridInvoices_CellPainting;
            gridInvoices.CellClick += GridInvoices_CellClick;
            gridInvoices.CellFormatting += GridInvoices_CellFormatting;

            InitializeGridColumns();
            this.Controls.Add(gridInvoices);

            // Kolon Ayarları butonu - sağ üstte sabit
            btnColumnSettings = new Button
            {
                Text = "Kolon Ayarları",
                Location = new Point(1060, 102),
                Size = new Size(120, 26),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnColumnSettings.Click += BtnColumnSettings_Click;
            this.Controls.Add(btnColumnSettings);

            // Transfer Button - sağ altta sabit, pencere büyüyünce yerinde kalır (form yüksekliği 700, alt margin 20)
            btnTransfer = new Button
            {
                Text = "Aktar",
                Location = new Point(1060, 645),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 9.75F, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnTransfer.Click += BtnTransfer_Click;
            this.Controls.Add(btnTransfer);

            // Alt orta: logo (yazının üstünde) + "Yazılım: Şirket Adı"
            picLogoFooter = new PictureBox
            {
                Size = new Size(140, 36),
                SizeMode = PictureBoxSizeMode.Zoom,
                Anchor = AnchorStyles.Bottom
            };
            try
            {
                string basePath = Application.StartupPath;
                string logoPath = Path.Combine(basePath, "logo.png");
                if (!File.Exists(logoPath))
                    logoPath = Path.Combine(basePath, "Assets", "logo.png");
                if (File.Exists(logoPath))
                    picLogoFooter.Image = Image.FromFile(logoPath);
            }
            catch { /* logo yoksa devam et */ }
            this.Controls.Add(picLogoFooter);

            lblSoftwareBy = new Label
            {
                Text = "Yazılım: " + SOFTWARE_COMPANY_NAME,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                AutoSize = true,
                Anchor = AnchorStyles.Bottom
            };
            this.Controls.Add(lblSoftwareBy);
        }


        /// <summary>
        /// Grid kolonlarını yapılandırır. Görünürlük kullanıcı ayarından okunur (Kolon Ayarları).
        /// </summary>
        private void InitializeGridColumns()
        {
            gridInvoices.Columns.Clear();
            
            // Checkbox column için RowHeader kullanıyoruz
            var definitions = GridColumnSettings.GetAllDefinitions();
            var visibleNames = GridColumnSettings.GetVisibleFieldNames();
            
            // ÖNEMLI: Eğer visibleNames NULL veya BOŞ ise = TÜM kolonlar görünsün
            // Eğer visibleNames dolu ise = SADECE o listede OLAN kolonlar görünsün
            bool allVisible = (visibleNames == null || visibleNames.Count == 0);

            foreach (var def in definitions)
            {
                // Kullanıcı ayarlarına göre görünürlük belirlenir
                bool shouldBeVisible = allVisible || visibleNames.Contains(def.FieldName);
                
                // SADECE görünür olması gereken kolonları ekle
                if (!shouldBeVisible)
                    continue; // Bu kolonu ekleme
                
                DataGridViewColumn col;
                if (def.FieldName == "InvoiceDate" || def.FieldName == "DueDate" || def.FieldName == "TransferDate")
                {
                    col = new DataGridViewTextBoxColumn
                    {
                        DataPropertyName = def.FieldName,
                        HeaderText = def.Caption,
                        Visible = true,
                        Width = 100,
                        MinimumWidth = 70,
                        DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" }
                    };
                }
                else if (def.FieldName == "TotalAmount" || def.FieldName == "GenelToplam" || def.FieldName == "Bruttutar" || def.FieldName == "TaxAmount" ||
                    def.FieldName == "SatIskt" || def.FieldName == "MfazIskt" || def.FieldName == "GenIsk1T" || def.FieldName == "GenIsk1O" ||
                    def.FieldName == "GenIsk2T" || def.FieldName == "GenIsk2O" || def.FieldName == "GenIsk3T" || def.FieldName == "GenIsk3O")
                {
                    col = new DataGridViewTextBoxColumn
                    {
                        DataPropertyName = def.FieldName,
                        HeaderText = def.Caption,
                        Visible = true,
                        Width = 100,
                        MinimumWidth = 70,
                        DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
                    };
                }
                else if (def.FieldName == "IsTransferredToNetsis")
                {
                    col = new DataGridViewTextBoxColumn
                    {
                        DataPropertyName = def.FieldName,
                        HeaderText = def.Caption,
                        Visible = true,
                        Width = 100,
                        MinimumWidth = 70
                    };
                }
                else
                {
                    col = new DataGridViewTextBoxColumn
                    {
                        DataPropertyName = def.FieldName,
                        HeaderText = def.Caption,
                        Visible = true, // Zaten sadece görünür olanları ekliyoruz
                        Width = 100,
                        MinimumWidth = 70
                    };
                }
                
                gridInvoices.Columns.Add(col);
            }
        }

        private void BtnColumnSettings_Click(object sender, EventArgs e)
        {
            using (var form = new GridColumnSettingsForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;
                if (form.SelectedVisibleFieldNames != null)
                {
                    // Ayarları kaydet
                    GridColumnSettings.SetVisibleFieldNames(form.SelectedVisibleFieldNames);
                    
                    // Mevcut veriyi sakla
                    var currentData = gridInvoices.DataSource;
                    
                    // Grid'i tamamen yeniden oluştur
                    gridInvoices.DataSource = null;
                    gridInvoices.Columns.Clear(); // Önce tüm kolonları temizle
                    
                    // Kolonları yeniden oluştur (yeni ayarlara göre)
                    InitializeGridColumns();
                    
                    // Veriyi geri yükle
                    gridInvoices.DataSource = currentData;
                    gridInvoices.Refresh();
                }
            }
        }

        // Checkbox için RowHeader'ı kullanma
        private HashSet<int> _selectedRowIndices = new HashSet<int>();

        private void GridInvoices_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Sol üst köşe: Tümünü seç / kaldır checkbox
            if (e.RowIndex == -1 && e.ColumnIndex == -1)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                int rowCount = gridInvoices.RowCount;
                bool allSelected = rowCount > 0 && _selectedRowIndices.Count == rowCount;
                var checkBoxSize = new Size(15, 15);
                var checkBoxLocation = new Point(
                    e.CellBounds.Location.X + (e.CellBounds.Width / 2 - checkBoxSize.Width / 2),
                    e.CellBounds.Location.Y + (e.CellBounds.Height / 2 - checkBoxSize.Height / 2));
                var checkBoxState = allSelected
                    ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal
                    : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;
                CheckBoxRenderer.DrawCheckBox(e.Graphics, checkBoxLocation, checkBoxState);
                e.Handled = true;
                return;
            }
            // Satır başlığı: satır checkbox
            if (e.RowIndex >= 0 && e.ColumnIndex == -1)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                var checkBoxSize = new Size(15, 15);
                var checkBoxLocation = new Point(
                    e.CellBounds.Location.X + (e.CellBounds.Width / 2 - checkBoxSize.Width / 2),
                    e.CellBounds.Location.Y + (e.CellBounds.Height / 2 - checkBoxSize.Height / 2));
                var checkBoxState = _selectedRowIndices.Contains(e.RowIndex)
                    ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal
                    : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;
                CheckBoxRenderer.DrawCheckBox(e.Graphics, checkBoxLocation, checkBoxState);
                e.Handled = true;
            }
        }

        private void GridInvoices_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Sol üst köşe: tümünü seç / kaldır
            if (e.RowIndex == -1 && e.ColumnIndex == -1)
            {
                int rowCount = gridInvoices.RowCount;
                if (rowCount == 0) return;
                bool allSelected = _selectedRowIndices.Count == rowCount;
                if (allSelected)
                {
                    _selectedRowIndices.Clear();
                }
                else
                {
                    _selectedRowIndices.Clear();
                    for (int i = 0; i < rowCount; i++)
                        _selectedRowIndices.Add(i);
                }
                gridInvoices.Invalidate();
                return;
            }
            // Satır başlığı: tek satır seç/kaldır
            if (e.RowIndex >= 0 && e.ColumnIndex == -1)
            {
                if (_selectedRowIndices.Contains(e.RowIndex))
                    _selectedRowIndices.Remove(e.RowIndex);
                else
                    _selectedRowIndices.Add(e.RowIndex);
                gridInvoices.InvalidateRow(e.RowIndex);
                gridInvoices.InvalidateCell(-1, -1); // Köşe checkbox'ı güncelle
            }
        }

        /// <summary>
        /// Aktarım Durumu kolonunda True/False yerine "Aktarılan" / "Aktarılmayan" gösterir.
        /// </summary>
        private void GridInvoices_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var col = gridInvoices.Columns[e.ColumnIndex];
            if (col == null) return;
            if (col.DataPropertyName == "IsTransferredToNetsis")
            {
                if (e.Value is bool transferred)
                {
                    e.Value = transferred ? "Aktarılan" : "Aktarılmayan";
                    e.FormattingApplied = true;
                }
                return;
            }
            if (col.DataPropertyName == "KdvDahilmi")
            {
                if (e.Value == null || e.Value == DBNull.Value)
                {
                    e.Value = "-";
                    e.FormattingApplied = true;
                    return;
                }
                if (e.Value is bool kdvDahil)
                {
                    e.Value = kdvDahil ? "Evet" : "Hayır";
                    e.FormattingApplied = true;
                }
                else
                {
                    e.Value = "-";
                    e.FormattingApplied = true;
                }
            }
        }

        #endregion

        #region Data Loading Methods


        /// <summary>
        /// Başlangıçta veriyi arka planda yükler; form hemen açılır, SQL uyarısı çıkmaz.
        /// </summary>
        private void LoadInitialDataAsync()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                List<InvoiceDto> list = null;
                Exception error = null;
                try
                {
                    DateTime startDate = DateTime.Now.AddMonths(-1);
                    DateTime endDate = DateTime.Now;
                    list = GetInvoicesFromNetsis(startDate, endDate);
                }
                catch (Exception ex)
                {
                    error = ex;
                }

                this.BeginInvoke(new Action(() =>
                {
                    if (gridInvoices == null) return;

                    if (error != null)
                    {
                        _invoiceList = new List<InvoiceDto>();
                        gridInvoices.DataSource = _mapper.Map<List<InvoiceListViewModel>>(_invoiceList);
                        _selectedRowIndices.Clear();
                        this.Text = "Fatura Aktarım - Netsis bağlantısı yok (Getir ile tekrar deneyin)";
                        return;
                    }

                    _invoiceList = list ?? new List<InvoiceDto>();
                    var displayList = _mapper.Map<List<InvoiceListViewModel>>(_invoiceList);
                    gridInvoices.DataSource = displayList;
                    _selectedRowIndices.Clear();

                    if (_invoiceList.Count == 0)
                        this.Text = "Fatura Aktarım - Netsis bağlantısı bekleniyor";
                    else
                        this.Text = $"Fatura Aktarım - {_invoiceList.Count} fatura";
                }));
            });
        }

        private void GetSalesInvoices()
        {
            try
            {
                DateTime startDate = dateStartDate.Value.Date;
                DateTime endDate = dateEndDate.Value.Date;

                if (startDate > endDate)
                {
                    MessageBox.Show("Başlangıç tarihi bitiş tarihinden büyük olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _invoiceList = GetInvoicesFromNetsis(startDate, endDate);
                if (_invoiceList == null || _invoiceList.Count == 0)
                {
                    _invoiceList = new List<InvoiceDto>();
                    gridInvoices.DataSource = _mapper.Map<List<InvoiceListViewModel>>(_invoiceList);
                    _selectedRowIndices.Clear();
                    MessageBox.Show(
                        "Seçilen tarih aralığında fatura bulunamadı.\n\nNot: Netsis entegrasyonu henüz tamamlanmadıysa,\nfaturalar görüntülenemez.",
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Filtreleri temizle
                cmbStatus.SelectedIndexChanged -= CmbStatus_SelectedIndexChanged;
                txtSearch.TextChanged -= TxtSearch_TextChanged;
                
                cmbStatus.SelectedIndex = 0;
                txtSearch.Text = "";
                
                cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;
                txtSearch.TextChanged += TxtSearch_TextChanged;

                // Grid'i güncelle
                var displayList = _mapper.Map<List<InvoiceListViewModel>>(_invoiceList);
                gridInvoices.DataSource = displayList;
                _selectedRowIndices.Clear();

                this.Text = $"Fatura Aktarım - {_invoiceList.Count} fatura";
                MessageBox.Show($"{_invoiceList.Count} adet fatura getirildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Faturalar getirilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<InvoiceDto> GetInvoicesFromNetsis(DateTime startDate, DateTime endDate)
        {
            return _invoiceService.GetSalesInvoices(SOURCE_COMPANY, startDate, endDate);
        }

        #endregion

        #region Transfer Methods

        private async System.Threading.Tasks.Task TransferToNetsisAsync()
        {
            try
            {
                var selectedIds = GetSelectedInvoiceIds();
                if (selectedIds == null || selectedIds.Count == 0)
                {
                    MessageBox.Show("Lütfen aktarılacak faturaları seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show($"{selectedIds.Count} adet fatura Netsis'e aktarılacak. Devam etmek istiyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                // Butonu devre dışı bırak
                btnTransfer.Enabled = false;
                btnTransfer.Text = "Aktarılıyor...";

                int successCount = 0, failureCount = 0;
                var errors = new List<string>();

                foreach (var invoiceId in selectedIds)
                {
                    try
                    {
                        var invoiceDto = _invoiceList?.FirstOrDefault(x => x.Id == invoiceId);
                        string netsisReferenceNo = await _invoiceService.TransferInvoiceAsync(invoiceId, TARGET_COMPANY);
                        if (invoiceDto != null)
                        {
                            invoiceDto.IsTransferredToNetsis = true;
                            invoiceDto.TransferDate = DateTime.Now;
                            invoiceDto.NetsisReferenceNumber = netsisReferenceNo;
                        }
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        var invoiceDto = _invoiceList?.FirstOrDefault(x => x.Id == invoiceId);
                        errors.Add($"Fatura {invoiceDto?.InvoiceNumber ?? invoiceId.ToString()}: {ex.Message}");
                    }
                }

                string resultMessage = $"Aktarım tamamlandı!\n\n✅ Başarılı: {successCount} fatura\n❌ Başarısız: {failureCount} fatura";
                if (errors.Count > 0)
                {
                    resultMessage += "\n\nHatalar:\n" + string.Join("\n", errors.Take(5));
                    if (errors.Count > 5) resultMessage += $"\n... ve {errors.Count - 5} hata daha";
                }
                MessageBox.Show(resultMessage, failureCount > 0 ? "Kısmi Başarı" : "Başarılı", MessageBoxButtons.OK, failureCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                RefreshInvoiceGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Aktarım sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Butonu tekrar etkinleştir
                btnTransfer.Enabled = true;
                btnTransfer.Text = "Aktar";
            }
        }

        private List<int> GetSelectedInvoiceIds()
        {
            var selectedIds = new List<int>();
            var dataSource = gridInvoices.DataSource as List<InvoiceListViewModel>;
            if (dataSource == null) return selectedIds;

            foreach (var rowIndex in _selectedRowIndices)
            {
                if (rowIndex >= 0 && rowIndex < dataSource.Count)
                {
                    selectedIds.Add(dataSource[rowIndex].Id);
                }
            }
            return selectedIds;
        }

        private void RefreshInvoiceGrid() => ApplyFilters();

        #endregion

        #region Event Handlers

        private void BtnGetInvoices_Click(object sender, EventArgs e) => GetSalesInvoices();
        private async void BtnTransfer_Click(object sender, EventArgs e) => await TransferToNetsisAsync();
        private void TxtSearch_TextChanged(object sender, EventArgs e) => ApplyFilters();
        private void CmbStatus_SelectedIndexChanged(object sender, EventArgs e) => ApplyFilters();

        private void ApplyFilters()
        {
            if (gridInvoices == null || _invoiceList == null) return;

            var filtered = _invoiceList.AsEnumerable();
            int durumIndex = cmbStatus?.SelectedIndex ?? 0;
            if (durumIndex == 1) filtered = filtered.Where(x => !x.IsTransferredToNetsis);
            else if (durumIndex == 2) filtered = filtered.Where(x => x.IsTransferredToNetsis);

            string searchText = (txtSearch?.Text ?? "").Trim().ToLower();
            if (searchText.Length > 0)
                filtered = filtered.Where(inv =>
                    (inv.InvoiceNumber?.ToLower().Contains(searchText) ?? false) ||
                    (inv.CustomerName?.ToLower().Contains(searchText) ?? false) ||
                    (inv.CustomerCode?.ToLower().Contains(searchText) ?? false) ||
                    (inv.Description?.ToLower().Contains(searchText) ?? false) ||
                    (inv.Status?.ToLower().Contains(searchText) ?? false));

            var displayList = _mapper.Map<List<InvoiceListViewModel>>(filtered.ToList());
            gridInvoices.DataSource = displayList;
            _selectedRowIndices.Clear();

            int aktarilan = _invoiceList.Count(x => x.IsTransferredToNetsis);
            int aktarilmayan = _invoiceList.Count(x => !x.IsTransferredToNetsis);
            this.Text = $"Fatura Aktarım - Toplam: {_invoiceList.Count} | Aktarılmayan: {aktarilmayan} | Aktarılan: {aktarilan} | Gösterilen: {displayList.Count}";
        }

        private void GridViewInvoices_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (gridInvoices.CurrentRow == null || gridInvoices.CurrentRow.Index < 0) return;
                
                var vm = gridInvoices.CurrentRow.DataBoundItem as InvoiceListViewModel;
                if (vm != null)
                {
                    // Popup için fatura + kalemler Netsis'ten GetInvoiceById ile yüklenir
                    var invoiceDto = _invoiceService.GetInvoiceById(vm.Id);
                    if (invoiceDto != null)
                    {
                        var detailVm = _mapper.Map<InvoiceDetailViewModel>(invoiceDto);
                        for (int i = 0; i < (detailVm.Items?.Count ?? 0); i++)
                            detailVm.Items[i].ItemNo = i + 1;
                        using (var detailForm = new InvoiceDetailForm(detailVm, _panelUser))
                            detailForm.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fatura detayı gösterilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Form Designer Generated Code

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 670);
            this.Name = "MainForm";
            this.Text = "Fatura Aktarım";
            this.ResumeLayout(false);
        }

        #endregion
    }
}
