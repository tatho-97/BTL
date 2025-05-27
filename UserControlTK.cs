using System;
using System.Data;
using System.Windows.Forms;

namespace BTL
{
    public partial class UserControlTK : UserControl
    {
        private readonly Database _db = new();
        private DataTable _dtTK;
        private DataView _viewTK;
        private enum Mode { View, Add, ChangePass }
        private Mode _mode = Mode.View;

        public UserControlTK()
        {
            InitializeComponent();
            Load += UserControlTK_Load;
        }

        private void UserControlTK_Load(object? sender, EventArgs e)
        {
            LoadGrid();
            dataGridView.SelectionChanged += DataGridView_SelectionChanged;
            textBoxSearch.TextChanged += buttonSearch_Click;
            comboBoxGV.DisplayMember = "TenGV";
            comboBoxGV.ValueMember = "MaGV";
            comboBoxGV.DataSource = _db.GetAll<GiangVien>();

            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        private void LoadGrid()
        {
            string filter = _viewTK?.RowFilter ?? "";
            _dtTK = _db.GetAll<TaiKhoan>();
            _viewTK = new DataView(_dtTK) { RowFilter = filter };
            dataGridView.DataSource = _viewTK;
            dataGridView.Columns["Username"].HeaderText = "Tên đăng nhập";
            dataGridView.Columns["Password"].HeaderText = "Mật khẩu";
            dataGridView.Columns["Role"].HeaderText = "Phân quyền";
            dataGridView.Columns["TenGV"].HeaderText = "Giảng viên";
            dataGridView.Columns["MaGV"].Visible = false;
            dataGridView.Columns["MaKhoa"].Visible = false;

            // Tự động co giãn cột
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            // Tuỳ chỉnh tỉ lệ mỗi cột
            dataGridView.Columns["Username"].FillWeight = 25;
            dataGridView.Columns["Password"].FillWeight = 25;
            dataGridView.Columns["Role"].FillWeight = 20;
            dataGridView.Columns["TenGV"].FillWeight = 30;
        }

        private void DataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            var row = ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row;
            textBox1.Text = row["Username"].ToString();
            textBox2.Text = row["Password"].ToString();
            textBox3.Text = row["Role"].ToString();
            comboBoxGV.Text = row["TenGV"].ToString();
        }

        private void buttonSearch_Click(object? sender, EventArgs e)
        {
            string kw = textBoxSearch.Text.Replace("'", "''").Trim();
            _viewTK.RowFilter = string.IsNullOrEmpty(kw) ? "" :
                $"Username LIKE '%{kw}%' OR Role LIKE '%{kw}%'";
        }

        // ----------------------- CRUD buttons --------------------
        private void buttonThem_Click(object? sender, EventArgs e)
        {
            _mode = Mode.Add;
            ClearInput();
            ToggleEdit(true);
        }

        private void buttonDMK_Click(object? sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;

            // Chuyển chế độ đổi mật khẩu
            _mode = Mode.ChangePass;

            // 1) Dùng ToggleEdit để tắt grid, bật confirm/hủy, ẩn các nút thêm/xóa/đổi mật khẩu
            ToggleEdit(true);
        }

        private void buttonHuy_Click(object? sender, EventArgs e)
        {
            _mode = Mode.View;
            ToggleEdit(false);
            buttonXN.Visible = buttonHuy.Visible = false;
            buttonThem.Enabled = buttonXoa.Enabled = buttonDMK.Enabled = true;
            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        private void buttonXoa_Click(object? sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            string user = textBox1.Text;
            if (MessageBox.Show($"Xoá tài khoản {user}?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _db.Delete<TaiKhoan>(user);
                LoadGrid();
            }
        }

        // ----------------------- helper ---------------------------
        private void ToggleEdit(bool enable)
        {
            dataGridView.Enabled = !enable;
            textBox1.Enabled = (_mode == Mode.Add);
            textBox2.Enabled = enable;
            textBox3.Enabled = (_mode == Mode.Add);
            comboBoxGV.Enabled = (_mode == Mode.Add);
            buttonXN.Visible = enable;
            buttonHuy.Visible = enable;
            buttonThem.Visible = !enable;
            buttonXoa.Visible = !enable;
            buttonDMK.Visible = !enable;
        }

        private void ClearInput()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
        }

        private void buttonXN_Click(object sender, EventArgs e)
        {
            bool ok = false;
            if (_mode == Mode.Add)
            {
                if (_db.Exist<TaiKhoan>(textBox1.Text))
                {
                    MessageBox.Show("Username đã tồn tại!");
                    return;
                }
                var tk = new TaiKhoan
                {
                    Username = textBox1.Text.Trim(),
                    Password = textBox2.Text.Trim(),
                    Role = textBox3.Text.Trim(),
                    MaGV = comboBoxGV.SelectedValue?.ToString(),
                    MaKhoa = null
                };
                ok = _db.Insert<TaiKhoan>(tk);
            }
            else if (_mode == Mode.ChangePass)
            {
                ok = _db.UpdatePassword(textBox1.Text.Trim(), textBox2.Text.Trim());
            }

            MessageBox.Show(ok ? "Thành công" : "Không thành công");
            buttonHuy_Click(null!, EventArgs.Empty);
            LoadGrid();
        }
    }
}
