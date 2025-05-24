using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BTL
{
    public partial class UserControlSV : UserControl
    {
        private readonly Database _db = new Database();
        private DataTable _dtSinhVien = new();
        private DataView _viewSinhVien;          // <-- NEW
        private bool _binding = false;
        private enum Mode { View, Add, Edit }
        private Mode _mode = Mode.View;

        public UserControlSV()
        {
            InitializeComponent();
            Load += UserControlSV_Load;          // đăng ký sự kiện
        }

        private void UserControlSV_Load(object? sender, EventArgs e)
        {
            // 1. Lấy danh sách lớp
            comboBoxLop.DisplayMember = "TenLop";
            comboBoxLop.ValueMember = "MaLop";
            comboBoxLop.DataSource = _db.GetAllLop();

            // 2. Lấy & gán nguồn dữ liệu sinh viên
            _dtSinhVien = _db.GetAllSinhVien();
            _viewSinhVien = _dtSinhVien.DefaultView;      // <-- NEW
            dataGridView.DataSource = _viewSinhVien;      // <-- sửa

            // 3. Sự kiện
            dataGridView.SelectionChanged += DataGridView_SelectionChanged;
            buttonSearch.Click += buttonSearch_Click;          // <-- NEW
            textBoxSearch.TextChanged += buttonSearch_Click;          // tìm ngay khi gõ (tùy thích)

            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }


        private void DataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            if (_binding) return;                 // đang binding, bỏ qua
            if (dataGridView.CurrentRow == null) return;

            _binding = true;
            var row = ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row;

            textBoxMaSV.Text = row["MaSV"].ToString();
            textBoxTenSV.Text = row["TenSV"].ToString();
            textBoxDiaChi.Text = row["DiaChi"].ToString();
            dateTimePickerNgaySinh.Value =
                DateTime.TryParse(row["NgaySinh"].ToString(), out var d) ? d : DateTime.Today;

            var gt = row["GioiTinh"].ToString()?.Trim().ToLower();
            radioButtonNam.Checked = gt == "nam";
            radioButtonNu.Checked = gt == "nữ" || gt == "nu";

            comboBoxLop.SelectedValue = row["MaLop"];   // chọn đúng lớp

            _binding = false;
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            string kw = textBoxSearch.Text.Replace("'", "''").Trim();   // escape ’

            if (string.IsNullOrEmpty(kw))
            {
                _viewSinhVien.RowFilter = "";         // bỏ lọc
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

        // ==== SỰ KIỆN NÚT ============================================================

        private void buttonThem_Click(object sender, EventArgs e)
        {
            _mode = Mode.Add;
            ClearInput();
            SetEditMode(true);

            buttonXacNhan.Visible = buttonHuy.Visible = true;
            buttonSua.Visible = buttonXoa.Visible = false;
            buttonSua.Enabled = buttonXoa.Enabled = buttonThem.Enabled = false;
        }

        private void buttonSua_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            _mode = Mode.Edit;
            SetEditMode(true);

            buttonXacNhan.Visible = buttonHuy.Visible = true;
            buttonSua.Visible = buttonXoa.Visible = false;
            dataGridView.Enabled = false;
            buttonThem.Enabled = buttonXoa.Enabled = buttonSua.Enabled = false;
        }

        private void buttonHuy_Click(object sender, EventArgs e)
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

        private void buttonXacNhan_Click(object sender, EventArgs e)
        {
            // gom dữ liệu từ form
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
                if (_db.ExistMaSV(sv.MaSV))
                {
                    MessageBox.Show("Mã sinh viên đã tồn tại, hãy nhập mã khác!",
                                    "Trùng khóa chính", MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    textBoxMaSV.Focus();
                    return;                     // STOP tại đây
                }
                ok = _db.InsertSinhVien(sv);
            }
            else if (_mode == Mode.Edit)
            {
                ok = _db.UpdateSinhVien(sv);
            }

            MessageBox.Show(ok ? "Lưu thành công!" : "Thao tác thất bại!");
            buttonHuy_Click(null!, EventArgs.Empty);
            RefreshSinhVienGrid();
        }

        private void buttonXoa_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            string ma = textBoxMaSV.Text;

            if (MessageBox.Show($"Xoá sinh viên {ma} ?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                bool ok = _db.DeleteSinhVien(ma);
                MessageBox.Show(ok ? "Đã xoá!" : "Không xoá được!");
                RefreshSinhVienGrid();
            }
        }

        // ==== TIỆN ÍCH ==============================================================
        private void ClearInput()
        {
            textBoxMaSV.Clear(); textBoxTenSV.Clear(); textBoxDiaChi.Clear();
            dateTimePickerNgaySinh.Value = DateTime.Today;
            radioButtonNam.Checked = true;
            comboBoxLop.SelectedIndex = 0;
        }

        private void RefreshSinhVienGrid()
        {
            // 1) Ghi nhớ bộ lọc hiện tại (nếu có)
            string filter = _viewSinhVien?.RowFilter ?? "";


            // 2) Lấy dữ liệu mới
            _dtSinhVien = _db.GetAllSinhVien();

            // 3) Tạo DataView mới & áp dụng lại filter
            _viewSinhVien = new DataView(_dtSinhVien);
            _viewSinhVien.RowFilter = filter;

            // 4) Bind trở lại lưới
            dataGridView.DataSource = _viewSinhVien;

            // 5) Chọn lại dòng đầu (tuỳ ý)
            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }
    }
}
