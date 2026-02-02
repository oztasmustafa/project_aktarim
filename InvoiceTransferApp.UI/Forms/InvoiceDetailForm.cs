using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using InvoiceTransferApp.Core.ViewModel;

namespace InvoiceTransferApp.UI.Forms
{
    /// <summary>
    /// Fatura detay ve satış kalemleri gösterim formu (ViewModel + PanelUserDto ile)
    /// </summary>
    public class InvoiceDetailForm : Form
    {
        #region Fields

        private Label lblTitle;
        private Label lblInvoiceNumber;
        private Label lblInvoiceDate;
        private Label lblCustomer;
        private Label lblTotal;
        private Label lblTransferDate;
        private Label lblItemsTitle;
        
        private TextBox txtInvoiceNumber;
        private TextBox txtInvoiceDate;
        private TextBox txtCustomer;
        private TextBox txtTotal;
        private TextBox txtTransferDate;
        
        private DataGridView gridItems;
        
        private Button btnClose;

        private InvoiceDetailViewModel _viewModel;
        private PanelUserDto _panelUser;

        #endregion

        #region Constructor

        public InvoiceDetailForm(InvoiceDetailViewModel viewModel, PanelUserDto panelUser)
        {
            _viewModel = viewModel;
            _panelUser = panelUser;
            InitializeComponent();
            InitializeCustomComponents();
            LoadInvoiceData();
        }

        #endregion

        #region Initialization

        private void InitializeCustomComponents()
        {
            this.Text = "Fatura Detayı";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label
            {
                Text = "Fatura Detay Bilgileri",
                Location = new Point(20, 20),
                Size = new Size(860, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            lblInvoiceNumber = new Label 
            { 
                Text = "Fatura No:", 
                Location = new Point(20, 70), 
                Size = new Size(100, 20), 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold) 
            };
            this.Controls.Add(lblInvoiceNumber);
            txtInvoiceNumber = new TextBox 
            { 
                Location = new Point(130, 67), 
                Size = new Size(200, 20), 
                ReadOnly = true 
            };
            this.Controls.Add(txtInvoiceNumber);

            lblInvoiceDate = new Label 
            { 
                Text = "Fatura Tarihi:", 
                Location = new Point(360, 70), 
                Size = new Size(100, 20), 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold) 
            };
            this.Controls.Add(lblInvoiceDate);
            txtInvoiceDate = new TextBox 
            { 
                Location = new Point(470, 67), 
                Size = new Size(150, 20), 
                ReadOnly = true 
            };
            this.Controls.Add(txtInvoiceDate);

            lblCustomer = new Label 
            { 
                Text = "Cari:", 
                Location = new Point(20, 110), 
                Size = new Size(100, 20), 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold) 
            };
            this.Controls.Add(lblCustomer);
            txtCustomer = new TextBox 
            { 
                Location = new Point(130, 107), 
                Size = new Size(490, 20), 
                ReadOnly = true 
            };
            this.Controls.Add(txtCustomer);

            lblTotal = new Label 
            { 
                Text = "Toplam Tutar:", 
                Location = new Point(650, 110), 
                Size = new Size(80, 20), 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold) 
            };
            this.Controls.Add(lblTotal);
            txtTotal = new TextBox 
            { 
                Location = new Point(740, 107), 
                Size = new Size(140, 20), 
                ReadOnly = true, 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), 
                ForeColor = Color.DarkGreen 
            };
            this.Controls.Add(txtTotal);

            lblTransferDate = new Label 
            { 
                Text = "Aktarım Tarihi:", 
                Location = new Point(20, 150), 
                Size = new Size(100, 20), 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), 
                Visible = false 
            };
            this.Controls.Add(lblTransferDate);
            txtTransferDate = new TextBox 
            { 
                Location = new Point(130, 147), 
                Size = new Size(200, 20), 
                ReadOnly = true, 
                ForeColor = Color.DarkBlue, 
                Visible = false 
            };
            this.Controls.Add(txtTransferDate);

            lblItemsTitle = new Label 
            { 
                Text = "Satış Kalemleri:", 
                Location = new Point(20, 190), 
                Size = new Size(200, 20), 
                Font = new Font("Segoe UI", 10F, FontStyle.Bold) 
            };
            this.Controls.Add(lblItemsTitle);

            gridItems = new DataGridView 
            { 
                Location = new Point(20, 220), 
                Size = new Size(860, 290),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White
            };
            InitializeItemsGrid();
            this.Controls.Add(gridItems);

            btnClose = new Button 
            { 
                Text = "Kapat", 
                Location = new Point(780, 525), 
                Size = new Size(100, 30) 
            };
            btnClose.Click += BtnClose_Click;
            this.Controls.Add(btnClose);
        }

        private void InitializeItemsGrid()
        {
            gridItems.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "ItemNo", 
                HeaderText = "Sıra", 
                Width = 50 
            });
            gridItems.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "ProductCode", 
                HeaderText = "Ürün Kodu", 
                Width = 120 
            });
            gridItems.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "ProductName", 
                HeaderText = "Ürün Adı", 
                Width = 300 
            });
            gridItems.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "Quantity", 
                HeaderText = "Miktar", 
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });
            gridItems.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "UnitPrice", 
                HeaderText = "Birim Fiyat", 
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });
            gridItems.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "LineTotal", 
                HeaderText = "Tutar", 
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });
            gridItems.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "TaxAmount", 
                HeaderText = "KDV Tutarı", 
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });
        }

        #endregion

        #region Data Loading

        private void LoadInvoiceData()
        {
            if (_viewModel == null) return;
            txtInvoiceNumber.Text = _viewModel.InvoiceNumber;
            txtInvoiceDate.Text = _viewModel.InvoiceDate.ToString("dd.MM.yyyy");
            txtCustomer.Text = $"{_viewModel.CustomerCode} - {_viewModel.CustomerName}";
            txtTotal.Text = $"{_viewModel.TotalAmount:N2} {_viewModel.Currency}";
            if (_viewModel.IsTransferredToNetsis && _viewModel.TransferDate.HasValue)
            {
                lblTransferDate.Visible = true;
                txtTransferDate.Visible = true;
                txtTransferDate.Text = _viewModel.TransferDate.Value.ToString("dd.MM.yyyy HH:mm");
            }
            
            var items = _viewModel.Items != null && _viewModel.Items.Count > 0 
                ? _viewModel.Items 
                : new List<InvoiceItemDetailViewModel>();
            
            gridItems.DataSource = items;
            
            // Toplam satırını footer olarak ekle (DataGridView'de manuel)
            if (items.Count > 0)
            {
                decimal total = 0;
                foreach (var item in items)
                    total += item.LineTotal;
                // StatusStrip veya Label ile gösterebiliriz, ama basit tutmak için şimdilik atlıyoruz
            }
        }

        #endregion

        #region Event Handlers

        private void BtnClose_Click(object sender, EventArgs e) => this.Close();

        #endregion

        #region Form Designer Generated Code

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 570);
            this.Name = "InvoiceDetailForm";
            this.Text = "Fatura Detayı";
            this.ResumeLayout(false);
        }

        #endregion
    }
}
