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
    public partial class UserControlCD : UserControl
    {
        private readonly Database _db = new();
        private string _username = "";
        private string _origPassword = "";

        public UserControlCD()
        {
            InitializeComponent();
            Load += UC_Load;

            // Gán sự kiện nút
            buttonDMK.Click += ButtonDMK_Click;
            buttonXacNhan.Click += ButtonXacNhan_Click;
            buttonHuy.Click += ButtonHuy_Click;
        }

        // 1. Hàm nạp tài khoản
        public void LoadAccount(string username)
        {
            _username = username;
            textBoxMaSV.Text = _username;                // Username

            // Lấy mật khẩu từ DB
            var dt = _db.GetDataTable(
                "SELECT Password FROM TaiKhoan WHERE Username=@u",
                ("@u", _username)
            );
            if (dt.Rows.Count > 0)
            {
                _origPassword = dt.Rows[0]["Password"].ToString() ?? "";
                textBox1.Text = _origPassword;         // Password
            }
            else
            {
                _origPassword = textBox1.Text = "";
            }

            // Thiết lập mặc định
            textBox1.Enabled = false;
            buttonXacNhan.Visible = false;
            buttonHuy.Visible = false;
            buttonDMK.Visible = true;
        }

        // 2. Đổi mật khẩu: bật edit
        private void ButtonDMK_Click(object? sender, EventArgs e)
        {
            textBox1.Enabled = true;
            buttonXacNhan.Visible = true;
            buttonHuy.Visible = true;
            buttonDMK.Visible = false;
            textBox1.Focus();
        }

        // 3. Xác nhận đổi mật khẩu
        private void ButtonXacNhan_Click(object? sender, EventArgs e)
        {
            string newPass = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(newPass))
            {
                MessageBox.Show("Mật khẩu không được để trống.", "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return;
            }

            bool ok = _db.UpdatePassword(_username, newPass);
            if (ok)
            {
                MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                _origPassword = newPass;
            }
            else
            {
                MessageBox.Show("Lỗi khi đổi mật khẩu.", "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Quay về chế độ view
            textBox1.Enabled = false;
            buttonXacNhan.Visible = false;
            buttonHuy.Visible = false;
            buttonDMK.Visible = true;
        }

        // 4. Hủy đổi mật khẩu: trả lại mật khẩu cũ
        private void ButtonHuy_Click(object? sender, EventArgs e)
        {
            textBox1.Text = _origPassword;
            textBox1.Enabled = false;
            buttonXacNhan.Visible = false;
            buttonHuy.Visible = false;
            buttonDMK.Visible = true;
        }

        // Thiết lập ban đầu khi Load Control
        private void UC_Load(object? sender, EventArgs e)
        {
            // Nếu muốn tự động nạp tài khoản của GV đang đăng nhập,
            // gọi LoadAccount(maGV) tại đây hoặc từ Form cha.
        }
    }
}
