namespace BTL
{
    public partial class FormLogin : Form
    {

        private readonly Database db = new Database();
        public FormLogin()
        {
            InitializeComponent();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            string user = textBoxUsername.Text.Trim();
            string pass = textBoxPassword.Text;          // đã đặt UseSystemPasswordChar trong Designer

            var tk = db.CheckLogin(user, pass);

            if (tk is null)
            {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!",
                                "Đăng nhập thất bại",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            // đăng nhập thành công
            Hide();                                      // ẩn FormLogin

            if (tk.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                using var f = new FormAdmin();
                f.ShowDialog();
            }
            else if (tk.Role.Equals("GV", StringComparison.OrdinalIgnoreCase))
            {
                using var f = new FormGV(tk.MaGV!); // truyền mã GV & khoa nếu cần
                f.ShowDialog();
            }
            else
            {
                MessageBox.Show($"Vai trò {tk.Role} chưa được hỗ trợ.");
            }

            textBoxPassword.Clear();                     // dọn dẹp
            Show();                                      // quay lại form login
        }

    }
}
