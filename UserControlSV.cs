using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace BTL
{
    public partial class UserControlSV : UserControl
    {
        private readonly Database _db = new();
        private DataTable _dtSinhVien = new();
        private DataView _viewSinhVien;          // dùng cho tìm kiếm
        private bool _binding = false;
        private enum Mode { View, Add, Edit }
        private Mode _mode = Mode.View;

        public UserControlSV()
        {
            InitializeComponent();
            Load += UserControlSV_Load;
        }

        private void UserControlSV_Load(object? sender, EventArgs e)
        {
            // 1. Lấy danh sách lớp
            comboBoxLop.DisplayMember = "TenLop";
            comboBoxLop.ValueMember = "MaLop";
            comboBoxLop.DataSource = _db.GetAll<Lop>();

            // 2. Lấy & gán nguồn dữ liệu sinh viên
            _dtSinhVien = _db.GetAll<SinhVien>();
            _viewSinhVien = _dtSinhVien.DefaultView;
            dataGridView.DataSource = _viewSinhVien;

            // Đổi tiêu đề cột cho dễ nhìn
            dataGridView.Columns["MaSV"].HeaderText = "Mã sinh viên";
            dataGridView.Columns["TenSV"].HeaderText = "Họ và tên";
            dataGridView.Columns["NgaySinh"].HeaderText = "Ngày sinh";
            dataGridView.Columns["GioiTinh"].HeaderText = "Giới tính";
            dataGridView.Columns["DiaChi"].HeaderText = "Địa chỉ";
            dataGridView.Columns["LopHoc"].HeaderText = "Lớp học";

            // Cho phép tự động co giãn cột để vừa với DataGridView
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            // (Tuỳ chọn) Đặt tỉ lệ fill riêng cho mỗi cột
            dataGridView.Columns["MaSV"].FillWeight = 10;   // 10%
            dataGridView.Columns["TenSV"].FillWeight = 30;  // 30%
            dataGridView.Columns["NgaySinh"].FillWeight = 15;  // 15%
            dataGridView.Columns["GioiTinh"].FillWeight = 10;  // 10%
            dataGridView.Columns["DiaChi"].FillWeight = 20;  // 20%
            dataGridView.Columns["LopHoc"].FillWeight = 15;   // 15%

            // Ẩn cột MaLop (khóa ngoại không cần hiển thị)
            if (dataGridView.Columns.Contains("MaLop"))
                dataGridView.Columns["MaLop"].Visible = false;

            // Sự kiện chọn dòng
            dataGridView.SelectionChanged += DataGridView_SelectionChanged;
            // Sự kiện tìm kiếm
            //buttonSearch.Click += ButtonSearch_Click;
            textBoxSearch.TextChanged += buttonSearch_Click;
            // Sự kiện nút CRUD
            buttonThem.Click += buttonThem_Click;
            buttonSua.Click += buttonSua_Click;
            buttonXoa.Click += buttonXoa_Click;
            buttonHuy.Click += buttonHuy_Click;
            buttonXacNhan.Click += buttonXacNhan_Click;

            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        // Hiển thị chi tiết sinh viên lên các ô input khi chọn dòng
        private void DataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            if (_binding || dataGridView.CurrentRow == null) return;
            _binding = true;
            var row = ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row;
            textBoxMaSV.Text = row["MaSV"].ToString();
            textBoxTenSV.Text = row["TenSV"].ToString();
            textBoxDiaChi.Text = row["DiaChi"].ToString();
            dateTimePickerNgaySinh.Value =
                DateTime.TryParse(row["NgaySinh"].ToString(), out var d) ? d : DateTime.Today;
            var gt = row["GioiTinh"].ToString()?.Trim().ToLower();
            radioButtonNam.Checked = gt == "nam";
            radioButtonNu.Checked = !radioButtonNam.Checked;
            comboBoxLop.SelectedValue = row["MaLop"];
            _binding = false;
        }

        // Tìm kiếm sinh viên theo Mã hoặc Tên
        private void buttonSearch_Click(object? sender, EventArgs e)
        {
            string kw = textBoxSearch.Text.Replace("'", "''").Trim();
            if (string.IsNullOrEmpty(kw))
            {
                _viewSinhVien.RowFilter = "";  // bỏ lọc
            }
            else
            {
                // Tìm theo Mã SV hoặc Tên SV (LIKE %% không phân biệt hoa-thường)
                _viewSinhVien.RowFilter =
                    $"MaSV LIKE '%{kw}%' OR TenSV LIKE '%{kw}%'";
            }
            // Khi kết quả lọc thay đổi → tự động cập nhật lưới
            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        private void ClearInput()
        {
            textBoxMaSV.Clear();
            textBoxTenSV.Clear();
            textBoxDiaChi.Clear();
            dateTimePickerNgaySinh.Value = DateTime.Today;
            radioButtonNam.Checked = true;
            comboBoxLop.SelectedIndex = 0;
        }

        private void SetEditMode(bool enable)
        {
            textBoxTenSV.Enabled =
            textBoxDiaChi.Enabled =
            dateTimePickerNgaySinh.Enabled =
            radioButtonNam.Enabled =
            radioButtonNu.Enabled =
            comboBoxLop.Enabled = enable;
            // MaSV chỉ cho phép nhập khi thêm mới
            textBoxMaSV.Enabled = (_mode == Mode.Add);
        }

        // ==== SỰ KIỆN CÁC NÚT CRUD ====

        private void buttonThem_Click(object? sender, EventArgs e)
        {
            _mode = Mode.Add;
            ClearInput();
            SetEditMode(true);
            buttonXacNhan.Visible = buttonHuy.Visible = true;
            buttonSua.Visible = buttonXoa.Visible = false;
            buttonThem.Enabled = buttonSua.Enabled = buttonXoa.Enabled = false;
        }

        private void buttonSua_Click(object? sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            _mode = Mode.Edit;
            SetEditMode(true);
            buttonXacNhan.Visible = buttonHuy.Visible = true;
            buttonSua.Visible = buttonXoa.Visible = false;
            dataGridView.Enabled = false;
            buttonThem.Enabled = buttonXoa.Enabled = buttonSua.Enabled = false;
        }

        private void buttonHuy_Click(object? sender, EventArgs e)
        {
            _mode = Mode.View;
            SetEditMode(false);
            buttonXacNhan.Visible = buttonHuy.Visible = false;
            buttonSua.Visible = buttonXoa.Visible = true;
            buttonThem.Enabled = buttonSua.Enabled = buttonXoa.Enabled = true;
            dataGridView.Enabled = true;
            // reload chọn dòng hiện tại
            DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        private void buttonXacNhan_Click(object? sender, EventArgs e)
        {
            // Gom dữ liệu từ form
            var sv = new SinhVien
            {
                MaSV = textBoxMaSV.Text.Trim(),
                TenSV = textBoxTenSV.Text.Trim(),
                NgaySinh = dateTimePickerNgaySinh.Value.ToString("yyyy-MM-dd"),
                GioiTinh = radioButtonNam.Checked ? "Nam" : "Nữ",
                DiaChi = textBoxDiaChi.Text.Trim(),
                MaLop = comboBoxLop.SelectedValue?.ToString()
            };

            bool ok = false;
            if (_mode == Mode.Add)
            {
                if (_db.Exist<SinhVien>(sv.MaSV))
                {
                    MessageBox.Show("Mã sinh viên đã tồn tại, hãy nhập mã khác!",
                                    "Trùng khóa chính", MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    textBoxMaSV.Focus();
                    return;  // Dừng thêm mới
                }
                ok = _db.Insert<SinhVien>(sv);
            }
            else if (_mode == Mode.Edit)
            {
                ok = _db.Update<SinhVien>(sv);
            }

            MessageBox.Show(ok ? "Lưu thành công!" : "Thao tác thất bại!");
            buttonHuy_Click(null!, EventArgs.Empty);
            RefreshSinhVienGrid();
        }

        private void buttonXoa_Click(object? sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            string ma = textBoxMaSV.Text;
            if (MessageBox.Show($"Xoá sinh viên {ma}?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _db.Delete<SinhVien>(ma);
                RefreshSinhVienGrid();
            }
        }

        private void RefreshSinhVienGrid()
        {
            string filter = _viewSinhVien?.RowFilter ?? "";
            _dtSinhVien = _db.GetAll<SinhVien>();
            _viewSinhVien = new DataView(_dtSinhVien) { RowFilter = filter };
            dataGridView.DataSource = _viewSinhVien;
        }
    }
}
