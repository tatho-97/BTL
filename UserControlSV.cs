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
    }
}
