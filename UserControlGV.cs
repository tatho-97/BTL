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
    public partial class UserControlGV : UserControl
    {
        private readonly Database _db = new();
        private DataTable _dtGV = new();
        private DataView _viewGV;
        private DataTable _dtKhoa, _dtMon;
        private enum Mode { View, Add, Edit }
        private Mode _mode = Mode.View;

        public UserControlGV()
        {
            InitializeComponent();
            Load += UserControlGV_Load;
        }

        private void UserControlGV_Load(object? s, EventArgs e)
        {
            // -- KHOA
            _dtKhoa = _db.GetAllKhoa();
            comboBoxKhoa.DisplayMember = "TenKhoa";
            comboBoxKhoa.ValueMember = "MaKhoa";
            comboBoxKhoa.DataSource = _dtKhoa;
            comboBoxKhoa.SelectedIndexChanged += ComboBoxKhoaChanged;

            // -- GIẢNG VIÊN
            _dtGV = _db.GetAllGiangVien();
            _viewGV = _dtGV.DefaultView;
            dataGridView.DataSource = _viewGV;
            dataGridView.SelectionChanged += DataGridView_SelectionChanged;

            // -- TÌM KIẾM
            buttonSearch.Click += ButtonSearch_Click;
            textBoxSearch.TextChanged += ButtonSearch_Click;

            // -- NÚT CRUD
            //buttonThem.Click += ButtonThem_Click;
            //buttonSua.Click += ButtonSua_Click;
            //buttonXoa.Click += ButtonXoa_Click;
            //buttonHuy.Click += ButtonHuy_Click;
            //buttonXacNhan.Click += ButtonXacNhan_Click;

            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        private void ComboBoxKhoaChanged(object? s, EventArgs e)
        {
            if (comboBoxKhoa.SelectedValue == null) return;
            string mk = comboBoxKhoa.SelectedValue.ToString()!;
            _dtMon = _db.GetMonHocByKhoa(mk);
            comboBoxMH.DisplayMember = "TenMH";
            comboBoxMH.ValueMember = "MaMH";
            comboBoxMH.DataSource = _dtMon;
        }

        private void DataGridView_SelectionChanged(object? s, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            var row = ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row;

            textBoxMaGV.Text = row["MaGV"].ToString();
            textBoxTenGV.Text = row["TenGV"].ToString();

            comboBoxKhoa.SelectedValue = row["MaKhoa"];
            // sự kiện selectedIndexChanged sẽ tự load môn
            comboBoxMH.SelectedValue = row["MaMH"];

            // Lớp giảng dạy
            dataGridViewLH.DataSource =
                _db.GetLopByGiangVien(row["MaGV"].ToString());
        }

        private void ButtonSearch_Click(object? s, EventArgs e)
        {
            string kw = textBoxSearch.Text.Replace("'", "''").Trim();
            _viewGV.RowFilter = string.IsNullOrEmpty(kw)
                ? ""
                : $"MaGV LIKE '%{kw}%' OR TenGV LIKE '%{kw}%'";
        }

        private void RefreshGrid()
        {
            string filter = _viewGV.RowFilter;
            _dtGV = _db.GetAllGiangVien();
            _viewGV = new DataView(_dtGV) { RowFilter = filter };
            dataGridView.DataSource = _viewGV;
        }

        private void SetEdit(bool enable)
        {
            textBoxTenGV.Enabled =
            comboBoxKhoa.Enabled =
            comboBoxMH.Enabled = enable;
            textBoxMaGV.Enabled = (_mode == Mode.Add);
        }

        private void buttonThem_Click(object sender, EventArgs e)
        {
            _mode = Mode.Add;
            ClearInput();
            SetEdit(true);
            ToggleButtons(editing: true);
        }

        private void buttonXoa_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            string ma = textBoxMaGV.Text;
            if (MessageBox.Show($"Xoá GV {ma} ?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _db.DeleteGiangVien(ma);
                RefreshGrid();
            }
        }

        private void buttonHuy_Click(object sender, EventArgs e)
        {
            _mode = Mode.View;
            SetEdit(false);
            ToggleButtons(editing: false);
            DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        private void buttonXacNhan_Click(object sender, EventArgs e)
        {
            var gv = new GiangVien
            {
                MaGV = textBoxMaGV.Text.Trim(),
                TenGV = textBoxTenGV.Text.Trim(),
                MaKhoa = comboBoxKhoa.SelectedValue?.ToString(),
                MaMH = comboBoxMH.SelectedValue?.ToString()
            };

            if (_mode == Mode.Add)
            {
                if (_db.ExistMaGV(gv.MaGV))
                {
                    MessageBox.Show("Mã giảng viên đã tồn tại!");
                    textBoxMaGV.Focus(); return;
                }
                _db.InsertGiangVien(gv);
            }
            else if (_mode == Mode.Edit)
                _db.UpdateGiangVien(gv);
            buttonHuy_Click(null!, EventArgs.Empty);
            RefreshGrid();
        }

        private void buttonSua_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            _mode = Mode.Edit;
            SetEdit(true);
            ToggleButtons(editing: true);
        }

        private void ToggleButtons(bool editing)
        {
            buttonXacNhan.Visible = buttonHuy.Visible = editing;
            buttonThem.Enabled = buttonSua.Enabled = buttonXoa.Enabled = !editing;
            buttonThem.Visible = buttonSua.Visible = buttonXoa.Visible = !editing;
        }
        private void ClearInput()
        {
            textBoxMaGV.Clear(); textBoxTenGV.Clear();
            comboBoxKhoa.SelectedIndex = 0;
        }
    }
}
