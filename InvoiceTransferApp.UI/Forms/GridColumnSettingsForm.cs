using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InvoiceTransferApp.UI.Settings;

namespace InvoiceTransferApp.UI.Forms
{
    /// <summary>
    /// Gridde görünecek alanları seçmek için form. Müşteri görmek istemediği kolonu kaldırır.
    /// </summary>
    public class GridColumnSettingsForm : Form
    {
        private CheckedListBox _listColumns;
        private Button _btnOk;
        private Button _btnCancel;
        private List<GridColumnDefinition> _definitions;
        private List<string> _visibleFieldNames;

        public List<string> SelectedVisibleFieldNames { get; private set; }

        public GridColumnSettingsForm()
        {
            _definitions = GridColumnSettings.GetAllDefinitions();
            _visibleFieldNames = GridColumnSettings.GetVisibleFieldNames();
            BuildForm();
        }

        private void BuildForm()
        {
            this.Text = "Görüntülenecek Alanlar (Kolon Ayarları)";
            this.Size = new Size(420, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblInfo = new Label
            {
                Text = "Gridde görmek istediğiniz kolonları işaretleyin. İşaretsiz kolonlar listede görünmez.",
                Location = new Point(12, 12),
                Size = new Size(380, 32),
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(lblInfo);

            _listColumns = new CheckedListBox
            {
                Location = new Point(12, 48),
                Size = new Size(380, 374),
                CheckOnClick = true
            };
            _listColumns.Items.Clear();
            foreach (var def in _definitions)
            {
                _listColumns.Items.Add(def.Caption);
                bool visible = _visibleFieldNames == null || _visibleFieldNames.Count == 0 ||
                              _visibleFieldNames.Contains(def.FieldName);
                _listColumns.SetItemChecked(_listColumns.Items.Count - 1, visible);
            }
            this.Controls.Add(_listColumns);

            _btnOk = new Button
            {
                Text = "Tamam",
                Location = new Point(212, 432),
                Size = new Size(88, 28),
                DialogResult = DialogResult.OK
            };
            _btnOk.Click += BtnOk_Click;
            this.Controls.Add(_btnOk);

            _btnCancel = new Button
            {
                Text = "İptal",
                Location = new Point(308, 432),
                Size = new Size(84, 28),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(_btnCancel);

            this.AcceptButton = _btnOk;
            this.CancelButton = _btnCancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var selected = new List<string>();
            for (int i = 0; i < _listColumns.Items.Count && i < _definitions.Count; i++)
            {
                if (_listColumns.GetItemChecked(i))
                    selected.Add(_definitions[i].FieldName);
            }
            SelectedVisibleFieldNames = selected;
        }
    }
}
