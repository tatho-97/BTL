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
    public partial class UserControlMH : UserControl
    {
        private readonly Database _db = new();
        private DataTable _dtMH;
        private DataView _viewMH;
        private enum Mode { View, Add, Edit }
        private Mode _mode = Mode.View;

        public UserControlMH() { InitializeComponent(); Load += UserControlMH_Load; }

        // ------------------------- LOAD ------------------------------------
        private void UserControlMH_Load(object? sender, EventArgs e)
        {
            // -- Khoa combobox
            comboBoxKhoa.DisplayMember = "TenKhoa";
            comboBoxKhoa.ValueMember = "MaKhoa";
            comboBoxKhoa.DataSource = _db.GetAllKhoa();

            // -- MonHoc grid
            _dtMH = _db.GetAllMonHoc_Detail();
            _viewMH = _dtMH.DefaultView;
            dataGridView.DataSource = _viewMH;
            // — ĐỔI TIÊU ĐỀ CỘT & ẨN CỘT —
            dataGridView.Columns["MaMH"].HeaderText = "Mã môn học";
            dataGridView.Columns["TenMH"].HeaderText = "Tên môn học";
            dataGridView.Columns["TinChi"].HeaderText = "Tín chỉ";
            dataGridView.Columns["TenKhoa"].HeaderText = "Khoa";
            if (dataGridView.Columns.Contains("MaKhoa"))
                dataGridView.Columns["MaKhoa"].Visible = false;

            // — AUTO-SIZE & TỈ LỆ FILL —
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.Columns["MaMH"].FillWeight = 15;  // 15%
            dataGridView.Columns["TenMH"].FillWeight = 45;  // 45%
            dataGridView.Columns["TinChi"].FillWeight = 20;  // 20%
            dataGridView.Columns["TenKhoa"].FillWeight = 20;  // 20%

            dataGridView.SelectionChanged += DataGridView_SelectionChanged;

            // -- Search & buttons
            buttonSearch.Click += DoSearch; textBoxSearch.TextChanged += DoSearch;
            buttonThem.Click += ButtonThem_Click;
            buttonSua.Click += ButtonSua_Click;
            buttonXoa.Click += ButtonXoa_Click;
            buttonHuy.Click += ButtonHuy_Click;
            buttonXacNhan.Click += ButtonXacNhan_Click;

            if (dataGridView.Rows.Count > 0) DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        // ------------------------- VIEW ------------------------------------
        private void DataGridView_SelectionChanged(object? s, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            var row = ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row;

            textBoxMaMH.Text = row["MaMH"].ToString();
            textBoxTenMH.Text = row["TenMH"].ToString();
            textBoxTC.Text = row["TinChi"].ToString();
            comboBoxKhoa.SelectedValue = row["MaKhoa"];

            // giảng viên dạy môn
            dataGridViewGV.DataSource = _db.GetGiangVienByMon(row["MaMH"].ToString());
            // — ĐỔI TIÊU ĐỀ CỘT & ẨN CỘT —
            dataGridViewGV.Columns["MaGV"].HeaderText = "Mã giảng viên";
            dataGridViewGV.Columns["TenGV"].HeaderText = "Họ và tên";
            //dataGridViewGV.Columns["TenKhoa"].HeaderText = "Khoa";
            //dataGridViewGV.Columns["TenMH"].HeaderText = "Môn học";
            //if (dataGridViewGV.Columns.Contains("MaKhoa"))
            //    dataGridViewGV.Columns["MaKhoa"].Visible = false;
            //if (dataGridViewGV.Columns.Contains("MaMH"))
            //    dataGridViewGV.Columns["MaMH"].Visible = false;

            // — AUTO-SIZE & TỈ LỆ FILL —
            dataGridViewGV.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewGV.Columns["MaGV"].FillWeight = 10;  // 10%
            dataGridViewGV.Columns["TenGV"].FillWeight = 40;  // 40%
            //dataGridViewGV.Columns["TenKhoa"].FillWeight = 25;  // 25%
            //dataGridViewGV.Columns["TenMH"].FillWeight = 25;  // 25%

        }

        // ------------------------- SEARCH ----------------------------------
        private void DoSearch(object? s, EventArgs e)
        {
            string kw = textBoxSearch.Text.Replace("'", "''").Trim();
            _viewMH.RowFilter = string.IsNullOrEmpty(kw)
                ? ""
                : $"MaMH LIKE '%{kw}%' OR TenMH LIKE '%{kw}%'";
        }

        // ------------------------- CRUD UI helper --------------------------
        private void ToggleEdit(bool editing)
        {
            textBoxTenMH.Enabled = textBoxTC.Enabled = comboBoxKhoa.Enabled = editing;
            textBoxMaMH.Enabled = (_mode == Mode.Add);

            buttonXacNhan.Visible = buttonHuy.Visible = editing;
            buttonThem.Enabled = buttonSua.Enabled = buttonXoa.Enabled = !editing;
            buttonThem.Visible = buttonSua.Visible = buttonXoa.Visible = !editing;
            dataGridView.Enabled = !editing;
        }
        private void ClearInput()
        {
            textBoxMaMH.Clear(); textBoxTenMH.Clear(); textBoxTC.Clear();
            comboBoxKhoa.SelectedIndex = 0;
        }
        private void RefreshGrid()
        {
            string filter = _viewMH.RowFilter;
            _dtMH = _db.GetAllMonHoc_Detail();
            _viewMH = new DataView(_dtMH) { RowFilter = filter };
            dataGridView.DataSource = _viewMH;
            //if (dataGridView.Rows.Count > 0) DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        // ------------------------- BUTTON HANDLERS -------------------------
        private void ButtonThem_Click(object? s, EventArgs e)
        { _mode = Mode.Add; ClearInput(); ToggleEdit(true); }

        private void ButtonSua_Click(object? s, EventArgs e)
        { if (dataGridView.CurrentRow == null) return; _mode = Mode.Edit; ToggleEdit(true); }

        private void ButtonHuy_Click(object? s, EventArgs e)
        { _mode = Mode.View; ToggleEdit(false); DataGridView_SelectionChanged(null!, EventArgs.Empty); }

        private void ButtonXoa_Click(object? s, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            string ma = textBoxMaMH.Text;
            if (MessageBox.Show($"Xoá môn {ma}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _db.DeleteMonHoc(ma);
                RefreshGrid();
            }
        }

        private void ButtonXacNhan_Click(object? s, EventArgs e)
        {
            var mh = new MonHoc
            {
                MaMH = textBoxMaMH.Text.Trim(),
                TenMH = textBoxTenMH.Text.Trim(),
                TinChi = int.TryParse(textBoxTC.Text, out var tc) ? tc : 0,
                MaKhoa = comboBoxKhoa.SelectedValue?.ToString()
            };
            if (_mode == Mode.Add)
            {
                if (_db.ExistMaMH(mh.MaMH)) { MessageBox.Show("Mã MH trùng!"); return; }
                _db.InsertMonHoc(mh);
            }
            else if (_mode == Mode.Edit) _db.UpdateMonHoc(mh);
            ButtonHuy_Click(null!, EventArgs.Empty);
            RefreshGrid();
        }
    }
}
