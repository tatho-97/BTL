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
    public partial class UserControlDiem : UserControl
    {
        private readonly Database _db = new();
        private DataTable _dtSV = new();
        private DataView _viewSV;
        private bool _binding = false;
        private bool _inEdit = false;
        private enum Mode { View, Edit }
        private Mode _mode = Mode.View;

        // sẽ nhận vào Mã GV từ FormGV
        public string _maGV = "";     // gán từ property hoặc ctor

        // ghi lại môn học đi kèm từng lớp để truy vấn điểm
        private readonly Dictionary<string, string> _lop_maMH = new();

        public UserControlDiem() { InitializeComponent(); Load += UC_Load; }

        // UC_Load: Khởi tạo UserControlDiem
        private void UC_Load(object? sender, EventArgs e)
        {
            // 1. Load danh sách lớp mà giảng viên dạy
            var dtLop = _db.GetLopByGiangVien(_maGV);
            comboBoxLop.DisplayMember = "TenLop";
            comboBoxLop.ValueMember = "MaLop";
            comboBoxLop.DataSource = dtLop;
            // Lưu mapping Lớp → Môn
            foreach (DataRow r in dtLop.Rows)
                _lop_maMH[r["MaLop"].ToString()] = r["MaMH"].ToString();
            comboBoxLop.SelectedIndexChanged += comboBoxLop_SelectedIndexChanged;

            // 2. Thiết lập lọc ký tự và làm tròn khi rời ô nhập điểm
            foreach (var tb in new[] { textBoxDiemCC, textBoxDiemTX, textBoxDiemTHI })
            {
                tb.KeyPress += OnlyNumber_KeyPress;
                tb.Leave += Diem_Leave;
            }

            // 3. Các sự kiện khác
            dataGridView.SelectionChanged += dataGridView_SelectionChanged;
            buttonSearch.Click += buttonSearch_Click;
            textBoxSearch.TextChanged += buttonSearch_Click;
            buttonSua.Click += buttonSua_Click;
            buttonXacNhan.Click += buttonXacNhan_Click;
            buttonHuy.Click += buttonHuy_Click;

            // 4. Kích hoạt ban đầu
            if (comboBoxLop.Items.Count > 0)
                comboBoxLop_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private static float Round05(float x) =>
    (float)(Math.Round(x * 2, MidpointRounding.AwayFromZero) / 2.0);

        // Sự kiện Leave
        private void Diem_Leave(object? sender, EventArgs e)
        {
            var tb = (TextBox)sender!;
            if (float.TryParse(tb.Text, out var v))
                tb.Text = Round05(v).ToString("0.0");
        }

        private void OnlyNumber_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;           // phím điều khiển
            var tb = (TextBox)sender!;
            if (char.IsDigit(e.KeyChar)) return;             // số 0–9
            if (e.KeyChar == '.' && !tb.Text.Contains('.'))  // 1 dấu chấm
                return;
            e.Handled = true;                                // chặn ký tự khác
        }

        private void comboBoxLop_SelectedIndexChanged(object? s, EventArgs e)
        {
            if (comboBoxLop.SelectedValue == null) return;

            string maLop = comboBoxLop.SelectedValue.ToString();
            string maMH = _lop_maMH[maLop];

            _dtSV = _db.GetSV_Diem_ByLop(maLop, maMH);      // viết ở §1
            _viewSV = _dtSV.DefaultView;
            dataGridView.DataSource = _viewSV;

            if (dataGridView.Rows.Count > 0)
                dataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        private void dataGridView_SelectionChanged(object? s, EventArgs e)
        {
            if (_inEdit || _binding || dataGridView.CurrentRow == null) return;
            _binding = true;

            var r = ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row;

            // thông tin SV
            textBoxMaSV.Text = r["MaSV"].ToString();
            textBoxTenSV.Text = r["TenSV"].ToString();
            textBoxDiaChi.Text = r["DiaChi"].ToString();
            dateTimePickerNgaySinh.Value =
                DateTime.TryParse(r["NgaySinh"].ToString(), out var d) ? d : DateTime.Today;
            var gt = r["GioiTinh"].ToString().Trim().ToLower();
            radioButtonNam.Checked = gt == "nam";
            radioButtonNu.Checked = !radioButtonNam.Checked;

            // điểm (có thể null)
            textBoxDiemCC.Text = r["DiemCC"]?.ToString();
            textBoxDiemTX.Text = r["DiemTX"]?.ToString();
            textBoxDiemTHI.Text = r["DiemTHI"]?.ToString();
            textBoxDiemHP.Text = r["DiemHP"]?.ToString();

            _binding = false;
        }

        private void buttonSearch_Click(object? s, EventArgs e)
        {
            string kw = textBoxSearch.Text.Replace("'", "''").Trim();
            _viewSV.RowFilter = string.IsNullOrEmpty(kw)
                ? ""
                : $"MaSV LIKE '%{kw}%' OR TenSV LIKE '%{kw}%'";
        }

        private void SetEditMode(bool enable)
        {
            // chỉ bật 4 ô điểm
            textBoxDiemCC.Enabled =
            textBoxDiemTX.Enabled =
            textBoxDiemTHI.Enabled = enable;
            // Điểm HP tự tính, không cho sửa
        }

        private void buttonSua_Click(object? s, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            _inEdit = true;
            _mode = Mode.Edit;

            // bật các TextBox để nhập điểm
            SetEditMode(true);

            // hiện nút Xác nhận, Hủy; ẩn nút Sửa
            buttonXacNhan.Visible = true;
            buttonHuy.Visible = true;
            buttonSua.Visible = false;
        }

        private void buttonHuy_Click(object? s, EventArgs e)
        {
            _inEdit = false;
            _mode = Mode.View;

            // khóa lại các TextBox điểm
            SetEditMode(false);

            // ẩn nút Xác nhận, Hủy; hiện nút Sửa
            buttonXacNhan.Visible = false;
            buttonHuy.Visible = false;
            buttonSua.Visible = true;

            // reload lại dữ liệu từ DataGridView
            dataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        // Xử lý nút Xác nhận: lưu/sửa điểm
        private void buttonXacNhan_Click(object? sender, EventArgs e)
        {
            // 1. Parse từng ô
            bool okCC = float.TryParse(textBoxDiemCC.Text, out var cc);
            bool okTX = float.TryParse(textBoxDiemTX.Text, out var tx);
            bool okTHI = float.TryParse(textBoxDiemTHI.Text, out var thi);

            // 2. Làm tròn từng giá trị nếu hợp lệ
            if (okCC) cc = Round05(cc);
            if (okTX) tx = Round05(tx);
            if (okTHI) thi = Round05(thi);

            // 3. Nếu thiếu bất kỳ đầu điểm nào → chỉ tính/hủy HP và dừng, không gọi DB
            if (!(okCC && okTX && okTHI))
            {
                // Xóa HP hiển thị
                textBoxDiemHP.Text = string.Empty;
                // Thông báo nhẹ
                MessageBox.Show("Chưa nhập đủ CC, TX và THI — không lưu điểm.");
                return;
            }

            // 4. Tính & làm tròn HP
            float hp = (float)Math.Round(0.1f * cc + 0.2f * tx + 0.7f * thi,
                                        1, MidpointRounding.AwayFromZero);
            textBoxDiemHP.Text = hp.ToString("0.0");

            // 5. Tiến hành lưu vào DB (chỉ khi cả 3 giá trị có)
            var d = new Diem
            {
                MaSV = textBoxMaSV.Text,
                MaMH = _lop_maMH[comboBoxLop.SelectedValue.ToString()],
                DiemCC = cc,
                DiemTX = tx,
                DiemTHI = thi,
                DiemHP = hp
            };

            bool ok = _db.ExistDiem(d.MaSV, d.MaMH)
                      ? _db.UpdateDiem(d)
                      : _db.InsertDiem(d);

            MessageBox.Show(ok ? "Lưu điểm thành công!" : "Lỗi khi lưu điểm.");

            // 6. Quay về chế độ View & reload
            buttonHuy_Click(this, EventArgs.Empty);
            comboBoxLop_SelectedIndexChanged(this, EventArgs.Empty);
        }
    }
}
