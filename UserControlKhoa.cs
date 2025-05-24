using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTL
{
    public partial class UserControlKhoa : UserControl
    {
        private readonly Database _db = new();
        private DataTable _dt; private DataView _view;
        private enum Mode { View, Add, Edit }
        private Mode _mode = Mode.View;

        public UserControlKhoa()
        {
            InitializeComponent();
            Load += UserControlKhoa_Load;
        }

        private void UserControlKhoa_Load(object? sender, System.EventArgs e)
        {
            _dt = _db.GetAllKhoa_Detail();
            _view = _dt.DefaultView;
            dataGridView.DataSource = _view;

            dataGridView.SelectionChanged += DataGridView_SelectionChanged;
            buttonSearch.Click += Search;
            textBoxSearch.TextChanged += Search;

            buttonThem.Click += BtnThem_Click;
            buttonXoa.Click += BtnXoa_Click;
            buttonXN.Click += BtnXN_Click;
            buttonHuy.Click += BtnHuy_Click;

            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null!, System.EventArgs.Empty);
        }

        private void DataGridView_SelectionChanged(object? s, System.EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            var row = ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row;
            textBox1.Text = row["MaKhoa"].ToString();
            textBox2.Text = row["TenKhoa"].ToString();
        }

        private void Search(object? s, System.EventArgs e)
        {
            string kw = textBoxSearch.Text.Replace("'", "''").Trim();
            _view.RowFilter = string.IsNullOrEmpty(kw) ? "" :
                $"MaKhoa LIKE '%{kw}%' OR TenKhoa LIKE '%{kw}%'";
        }

        private void BtnThem_Click(object? s, System.EventArgs e)
        {
            _mode = Mode.Add;
            ToggleEdit(true);
            ClearInput();
        }

        private void BtnHuy_Click(object? s, System.EventArgs e)
        {
            _mode = Mode.View;
            ToggleEdit(false);
            if (dataGridView.CurrentRow != null)
                DataGridView_SelectionChanged(null!, System.EventArgs.Empty);
        }

        private void BtnXN_Click(object? s, System.EventArgs e)
        {
            var k = new Khoa { MaKhoa = textBox1.Text.Trim(), TenKhoa = textBox2.Text.Trim() };
            bool ok = false;
            if (_mode == Mode.Add)
            {
                if (_db.ExistMaKhoa(k.MaKhoa)) { MessageBox.Show("Mã khoa đã tồn tại"); return; }
                ok = _db.InsertKhoa(k);
            }
            else if (_mode == Mode.Edit)
            {
                ok = _db.UpdateKhoa(k);
            }
            MessageBox.Show(ok ? "Lưu thành công" : "Thao tác thất bại");
            BtnHuy_Click(null!, System.EventArgs.Empty);
            RefreshGrid();
        }

        private void BtnXoa_Click(object? s, System.EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            string ma = textBox1.Text;
            if (MessageBox.Show($"Xoá khoa {ma}? Tất cả dữ liệu liên quan sẽ mất!", "Cảnh báo",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _db.DeleteKhoa(ma);
                RefreshGrid();
            }
        }

        private void RefreshGrid()
        {
            string f = _view.RowFilter;
            _dt = _db.GetAllKhoa_Detail();
            _view = new DataView(_dt) { RowFilter = f };
            dataGridView.DataSource = _view;
            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null!, System.EventArgs.Empty);
        }

        private void ToggleEdit(bool edit)
        {
            textBox2.Enabled = edit;
            textBox1.Enabled = (_mode == Mode.Add);
            buttonXN.Visible = buttonHuy.Visible = edit;
            buttonThem.Enabled = buttonXoa.Enabled = !edit;
        }

        private void ClearInput()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox1.Focus();
        }
    }
}
