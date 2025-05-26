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
    public partial class UserControlLH : UserControl
    {
        private readonly Database _db = new();

        private DataTable _dtLop;          // nguồn thô
        private DataView _viewLop;        // dùng lọc tìm kiếm

        private enum Mode { View, Add, Edit }
        private Mode _mode = Mode.View;

        public UserControlLH()
        {
            InitializeComponent();
            Load += UserControlLH_Load;
        }

        // ---------------------------------------------------------------------
        // 1. NẠP DỮ LIỆU
        // ---------------------------------------------------------------------
        private void UserControlLH_Load(object? sender, EventArgs e)
        {
            // Lớp học
            _dtLop = _db.GetAllLop_Detail();
            _viewLop = _dtLop.DefaultView;
            dataGridView.DataSource = _viewLop;
            dataGridView.SelectionChanged += DataGridView_SelectionChanged;
            // — ĐỔI TIÊU ĐỀ CỘT —
            if (dataGridView.Columns.Contains("MaLop"))
                dataGridView.Columns["MaLop"].HeaderText = "Mã lớp";
            if (dataGridView.Columns.Contains("TenLop"))
                dataGridView.Columns["TenLop"].HeaderText = "Tên lớp";
            // nếu view có luôn cả cột Khoa
            if (dataGridView.Columns.Contains("TenKhoa"))
                dataGridView.Columns["TenKhoa"].HeaderText = "Khoa";

            // — ẨN CỘT KHÓA NGOẠI (nếu có) —
            if (dataGridView.Columns.Contains("MaKhoa"))
                dataGridView.Columns["MaKhoa"].Visible = false;

            // — TỰ ĐỘNG CO GIÃN CỘT CHO VỪA VỚI GRID — 
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // — TUỲ CHỈNH TỈ LỆ CHIẾM CHỖ MỖI CỘT (FillWeight là phần trăm) —
            dataGridView.Columns["MaLop"].FillWeight = 20;   // 20%
            dataGridView.Columns["TenLop"].FillWeight = 50;   // 50%
            if (dataGridView.Columns.Contains("TenKhoa"))
                dataGridView.Columns["TenKhoa"].FillWeight = 30;   // 30%

            // (Tuỳ chọn) Nếu bạn muốn dòng tự động tăng chiều cao theo nội dung:
            // dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // Tìm kiếm
            buttonSearch.Click += DoSearch;
            textBoxSearch.TextChanged += DoSearch;

            // CRUD buttons
            //buttonThem.Click += ButtonThem_Click;
            //buttonSua.Click += ButtonSua_Click;
            //buttonXoa.Click += ButtonXoa_Click;
            //buttonHuy.Click += ButtonHuy_Click;
            //buttonXacNhan.Click += ButtonXacNhan_Click;

            //// Thêm môn học cho lớp
            //buttonMH.Click += ButtonMH_Click;

            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null, EventArgs.Empty);
        }

        // ------------------------------------------------------------------
        // 2. CHỌN 1 LỚP  →  đổ thông tin + danh sách SV, Môn
        // ------------------------------------------------------------------
        private void DataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            var row = ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row;

            textBoxMaLop.Text = row["MaLop"].ToString();
            textBoxTenLop.Text = row["TenLop"].ToString();

            string maLop = row["MaLop"].ToString();
            dataGridViewSV.DataSource = _db.GetSinhVienByLop(maLop);    // Đổi tiêu đề
            if (dataGridViewSV.Columns.Contains("MaSV"))
                dataGridViewSV.Columns["MaSV"].HeaderText = "Mã sinh viên";
            if (dataGridViewSV.Columns.Contains("TenSV"))
                dataGridViewSV.Columns["TenSV"].HeaderText = "Họ và tên";
            if (dataGridViewSV.Columns.Contains("NgaySinh"))
                dataGridViewSV.Columns["NgaySinh"].HeaderText = "Ngày sinh";
            if (dataGridViewSV.Columns.Contains("GioiTinh"))
                dataGridViewSV.Columns["GioiTinh"].HeaderText = "Giới tính";
            if (dataGridViewSV.Columns.Contains("DiaChi"))
                dataGridViewSV.Columns["DiaChi"].HeaderText = "Địa chỉ";
            dataGridViewSV.Columns["NgaySinh"].Visible = false;
            dataGridViewSV.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
           
            
            dataGridViewMH.DataSource = _db.GetMonByLop(maLop);
            if (dataGridViewMH.Columns.Contains("MaMH"))
                dataGridViewMH.Columns["MaMH"].HeaderText = "Mã môn học";
            if (dataGridViewMH.Columns.Contains("TenMH"))
                dataGridViewMH.Columns["TenMH"].HeaderText = "Tên môn học";
            if (dataGridViewMH.Columns.Contains("TenGV"))
                dataGridViewMH.Columns["TenGV"].HeaderText = "Giảng viên";
            dataGridViewMH.Columns["MaGV"].Visible = false;
            dataGridViewMH.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        // ------------------------------------------------------------------
        // 3. TÌM KIẾM
        // ------------------------------------------------------------------
        private void DoSearch(object? sender, EventArgs e)
        {
            string kw = textBoxSearch.Text.Replace("'", "''").Trim();
            _viewLop.RowFilter = string.IsNullOrEmpty(kw)
                ? string.Empty
                : $"MaLop LIKE '%{kw}%' OR TenLop LIKE '%{kw}%'";
        }

        // ------------------------------------------------------------------
        // 4. CRUD LỚP HỌC
        // ------------------------------------------------------------------
        private void buttonThem_Click(object? s, EventArgs e)
        {
            _mode = Mode.Add;
            ClearInput();
            SetEditMode(true);
        }

        private void buttonSua_Click(object? s, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            _mode = Mode.Edit;
            SetEditMode(true);
        }

        private void buttonHuy_Click(object? s, EventArgs e)
        {
            _mode = Mode.View;
            SetEditMode(false);
            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null, EventArgs.Empty);
        }

        private void buttonXacNhan_Click(object? s, EventArgs e)
        {
            var lp = new Lop
            {
                MaLop = textBoxMaLop.Text.Trim(),
                TenLop = textBoxTenLop.Text.Trim()
            };

            bool ok = false;
            if (_mode == Mode.Add)
            {
                if (_db.ExistMaLop(lp.MaLop))
                {
                    MessageBox.Show("Mã lớp đã tồn tại!");
                    textBoxMaLop.Focus();
                    return;
                }
                ok = _db.InsertLop(lp);
            }
            else if (_mode == Mode.Edit)
            {
                ok = _db.UpdateLop(lp);
            }

            MessageBox.Show(ok ? "Lưu thành công" : "Thao tác thất bại!");
            RefreshGrid();
            buttonHuy_Click(null, EventArgs.Empty);
        }

        private void buttonXoa_Click(object? s, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            string ma = textBoxMaLop.Text;
            if (MessageBox.Show($"Xoá lớp {ma}?", "Xác nhận",
                                 MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                == DialogResult.Yes)
            {
                _db.DeleteLop(ma);
                RefreshGrid();
            }
        }

        // ------------------------------------------------------------------
        // 5. THÊM MÔN HỌC CHO LỚP (GiangDay)
        // ------------------------------------------------------------------
        private void buttonMH_Click(object? s, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            string maLop = textBoxMaLop.Text;
            using var f = new FormThemMH(_db, maLop);
            if (f.ShowDialog() == DialogResult.OK)
                dataGridViewMH.DataSource = _db.GetMonByLop(maLop);
        }

        // ------------------------------------------------------------------
        // 6. Tiện ích
        // ------------------------------------------------------------------
        private void RefreshGrid()
        {
            string filter = _viewLop.RowFilter;
            _dtLop = _db.GetAllLop_Detail();
            _viewLop = new DataView(_dtLop) { RowFilter = filter };
            dataGridView.DataSource = _viewLop;
        }

        private void ClearInput()
        {
            textBoxMaLop.Clear();
            textBoxTenLop.Clear();
        }

        private void SetEditMode(bool enable)
        {
            textBoxTenLop.Enabled = enable;
            textBoxMaLop.Enabled = (_mode == Mode.Add);

            buttonXacNhan.Visible = buttonHuy.Visible = enable;
            buttonThem.Enabled = buttonSua.Enabled = buttonXoa.Enabled = !enable;
        }
    }
}
