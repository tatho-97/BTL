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

        // tên control trong Designer:
        // dataGridView, textBox1(Username), textBox2(Password), textBox3(Role), textBox4(TenGV)
        // buttonThem, buttonXoa, buttonDMK, buttonHuy, buttonXN, buttonSearch, textBoxSearch

        private void UserControlTK_Load(object? sender, EventArgs e)
        {
            LoadGrid();

            dataGridView.SelectionChanged += DataGridView_SelectionChanged;
            buttonSearch.Click += DoSearch; textBoxSearch.TextChanged += DoSearch;

            buttonThem.Click += ButtonThem_Click;   // thêm TK
            buttonXoa.Click += ButtonXoa_Click;    // xoá
            buttonDMK.Click += ButtonDMK_Click;    // đổi MK
            buttonHuy.Click += ButtonHuy_Click;
            buttonXN.Click += ButtonXN_Click;

            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        private void LoadGrid()
        {
            string filter = _viewTK?.RowFilter ?? "";
            _dtTK = _db.GetAllTaiKhoan();
            _viewTK = new DataView(_dtTK) { RowFilter = filter };
            dataGridView.DataSource = _viewTK;
        }

        private void DataGridView_SelectionChanged(object? s, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            var row = ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row;

            textBox1.Text = row["Username"].ToString();
            textBox2.Text = row["Password"].ToString();
            textBox3.Text = row["Role"].ToString();
            textBox4.Text = row["TenGV"].ToString();
        }

        private void DoSearch(object? s, EventArgs e)
        {
            string kw = textBoxSearch.Text.Replace("'", "''").Trim();
            _viewTK.RowFilter = string.IsNullOrEmpty(kw) ? "" :
                $"Username LIKE '%{kw}%' OR Role LIKE '%{kw}%'";
        }

        // ----------------------- CRUD buttons --------------------
        private void ButtonThem_Click(object? s, EventArgs e)
        {
            _mode = Mode.Add;
            ClearInput(); EnableEdit(true);
        }

        private void ButtonDMK_Click(object? s, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            _mode = Mode.ChangePass;
            textBox2.Enabled = true;         // chỉ cho sửa pass
            buttonXN.Visible = buttonHuy.Visible = true;
            buttonThem.Enabled = buttonXoa.Enabled = buttonDMK.Enabled = false;
        }

        private void ButtonHuy_Click(object? s, EventArgs e)
        {
            _mode = Mode.View;
            EnableEdit(false);
            buttonXN.Visible = buttonHuy.Visible = false;
            buttonThem.Enabled = buttonXoa.Enabled = buttonDMK.Enabled = true;
            if (dataGridView.Rows.Count > 0) DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        private void ButtonXN_Click(object? s, EventArgs e)
        {
            bool ok = false;
            if (_mode == Mode.Add)
            {
                if (_db.ExistUsername(textBox1.Text))
                {
                    MessageBox.Show("Username đã tồn tại!"); return;
                }
                var tk = new TaiKhoan
                {
                    Username = textBox1.Text.Trim(),
                    Password = textBox2.Text.Trim(),
                    Role = textBox3.Text.Trim(),
                    MaGV = null,
                    MaKhoa = null
                };
                ok = _db.InsertTaiKhoan(tk);
            }
            else if (_mode == Mode.ChangePass)
            {
                ok = _db.UpdatePassword(textBox1.Text.Trim(), textBox2.Text.Trim());
            }

            MessageBox.Show(ok ? "Thành công" : "Không thành công");
            ButtonHuy_Click(null!, EventArgs.Empty);
            LoadGrid();
        }

        private void ButtonXoa_Click(object? s, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            string user = textBox1.Text;
            if (MessageBox.Show($"Xoá tài khoản {user}?", "Xác nhận", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _db.DeleteTaiKhoan(user);
                LoadGrid();
            }
        }

        // ----------------------- helper ---------------------------
        private void EnableEdit(bool enable)
        {
            textBox1.Enabled = (_mode == Mode.Add);
            textBox2.Enabled = enable;
            textBox3.Enabled = enable;
            // textBox4 (TenGV) chỉ view → không cho edit

            buttonXN.Visible = buttonHuy.Visible = enable;
            buttonThem.Enabled = buttonXoa.Enabled = buttonDMK.Enabled = !enable;
        }
        private void ClearInput()
        {
            textBox1.Clear(); textBox2.Clear(); textBox3.Clear(); textBox4.Clear();
        }
    }
}
