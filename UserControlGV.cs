using System;
using System.Data;
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

        private void UserControlGV_Load(object? sender, EventArgs e)
        {
            // -- KHOA (ComboBox chọn khoa)
            _dtKhoa = _db.GetAll<Khoa>();
            comboBoxKhoa.DisplayMember = "TenKhoa";
            comboBoxKhoa.ValueMember = "MaKhoa";
            comboBoxKhoa.DataSource = _dtKhoa;
            comboBoxKhoa.SelectedIndexChanged += ComboBoxKhoaChanged;

            // -- GIẢNG VIÊN (lưới danh sách giảng viên)
            _dtGV = _db.GetAll<GiangVien>();
            _viewGV = _dtGV.DefaultView;
            dataGridView.DataSource = _viewGV;
            dataGridView.SelectionChanged += DataGridView_SelectionChanged;

            // Đổi tiêu đề cột hiển thị
                dataGridView.Columns["MaGV"].HeaderText = "Mã giảng viên";
                dataGridView.Columns["TenGV"].HeaderText = "Họ và tên";
                dataGridView.Columns["TenKhoa"].HeaderText = "Khoa";
                dataGridView.Columns["TenMH"].HeaderText = "Môn học";

            // Ẩn các cột mã khóa ngoại
                dataGridView.Columns["MaKhoa"].Visible = false;
                dataGridView.Columns["MaMH"].Visible = false;

            // Tự động điều chỉnh kích thước cột
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            // Tùy chỉnh tỉ lệ fill cho mỗi cột
            dataGridView.Columns["MaGV"].FillWeight = 10;
            dataGridView.Columns["TenGV"].FillWeight = 30;
                dataGridView.Columns["TenKhoa"].FillWeight = 30;
                dataGridView.Columns["TenMH"].FillWeight = 30;

            // Sự kiện tìm kiếm
            textBoxSearch.TextChanged += ButtonSearch_Click;

            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        private void ComboBoxKhoaChanged(object? sender, EventArgs e)
        {
            // Khi chọn khoa mới, cập nhật danh sách môn học tương ứng
            string maKhoa = comboBoxKhoa.SelectedValue?.ToString() ?? "";
            _dtMon = _db.GetMonHocByKhoa(maKhoa);
            comboBoxMH.DisplayMember = "TenMH";
            comboBoxMH.ValueMember = "MaMH";
            comboBoxMH.DataSource = _dtMon;
        }

        private void DataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            var row = ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row;
            textBoxMaGV.Text = row["MaGV"].ToString();
            textBoxTenGV.Text = row["TenGV"].ToString();
            comboBoxKhoa.SelectedValue = row["MaKhoa"];
            comboBoxMH.SelectedValue = row["MaMH"];
        }

        private void ButtonSearch_Click(object? sender, EventArgs e)
        {
            string kw = textBoxSearch.Text.Replace("'", "''").Trim();
            _viewGV.RowFilter = string.IsNullOrEmpty(kw)
                ? ""
                : $"MaGV LIKE '%{kw}%' OR TenGV LIKE '%{kw}%'";
        }

        private void ClearInput()
        {
            textBoxMaGV.Clear();
            textBoxTenGV.Clear();
            if (_dtKhoa.Rows.Count > 0) comboBoxKhoa.SelectedIndex = 0;
            if (_dtMon != null && _dtMon.Rows.Count > 0) comboBoxMH.SelectedIndex = 0;
        }

        private void ToggleEdit(bool editing)
        {
            dataGridView.Enabled = !editing;
            textBoxTenGV.Enabled = comboBoxKhoa.Enabled = comboBoxMH.Enabled = editing;
            textBoxMaGV.Enabled = (_mode == Mode.Add);
            buttonXacNhan.Visible = buttonHuy.Visible = editing;
            buttonSua.Visible = buttonThem.Visible = buttonXoa.Visible = !editing;
        }

        // === NÚT THÊM/SỬA/XÓA/HỦY/ XÁC NHẬN ===

        private void buttonThem_Click(object? sender, EventArgs e)
        {
            _mode = Mode.Add;
            ClearInput();
            ToggleEdit(true);
        }

        private void buttonSua_Click(object? sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            _mode = Mode.Edit;
            ToggleEdit(true);
        }

        private void buttonHuy_Click(object? sender, EventArgs e)
        {
            _mode = Mode.View;
            ToggleEdit(false);
            if (dataGridView.CurrentRow != null)
                DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        private void buttonXacNhan_Click(object? sender, EventArgs e)
        {
            var gv = new GiangVien
            {
                MaGV = textBoxMaGV.Text.Trim(),
                TenGV = textBoxTenGV.Text.Trim(),
                MaKhoa = comboBoxKhoa.SelectedValue?.ToString(),
                MaMH = comboBoxMH.SelectedValue?.ToString()
            };
            bool ok = false;
            if (_mode == Mode.Add)
            {
                if (_db.Exist<GiangVien>(gv.MaGV))
                {
                    MessageBox.Show("Mã giảng viên đã tồn tại!");
                    return;
                }
                ok = _db.Insert<GiangVien>(gv);
            }
            else if (_mode == Mode.Edit)
            {
                ok = _db.Update<GiangVien>(gv);
            }
            MessageBox.Show(ok ? "Lưu thành công" : "Thao tác thất bại!");
            buttonHuy_Click(null!, EventArgs.Empty);
            LoadGrid();
        }

        private void buttonXoa_Click(object? sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            string ma = textBoxMaGV.Text;
            if (MessageBox.Show($"Xoá giảng viên {ma}?", "Xác nhận", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _db.Delete<GiangVien>(ma);
                LoadGrid();
            }
        }

        private void LoadGrid()
        {
            string filter = _viewGV?.RowFilter ?? "";
            _dtGV = _db.GetAll<GiangVien>();
            _viewGV = new DataView(_dtGV) { RowFilter = filter };
            dataGridView.DataSource = _viewGV;
        }
    }
}
