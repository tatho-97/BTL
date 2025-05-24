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
    public partial class FormThemMH : Form
    {
        private readonly Database _db;
        private readonly string _maLop;

        public FormThemMH(Database db, string maLop)
        {
            InitializeComponent();
            _db = db; _maLop = maLop;

            // --- danh sách môn CHƯA được dạy trong lớp
            comboBoxMH.DisplayMember = "TenMH";
            comboBoxMH.ValueMember = "MaMH";
            comboBoxMH.DataSource = db.GetDataTable(
                @"SELECT MaMH, TenMH FROM MonHoc
                  WHERE MaMH NOT IN (SELECT MaMH FROM GiangDay WHERE MaLop=@lop);",
                ("@lop", maLop));

            // Khi chọn môn ⇒ nạp giảng viên dạy môn đó
            comboBoxMH.SelectedIndexChanged += (_, __) => LoadGV();
            LoadGV();

            buttonXacNhan.Click += ButtonXacNhan_Click;
        }

        private void LoadGV()
        {
            if (comboBoxMH.SelectedValue == null) return;
            comboBoxGV.DisplayMember = "TenGV";
            comboBoxGV.ValueMember = "MaGV";
            comboBoxGV.DataSource = _db.GetGiangVienByMon(comboBoxMH.SelectedValue.ToString());
        }

        private void ButtonXacNhan_Click(object? sender, EventArgs e)
        {
            if (comboBoxMH.SelectedValue == null || comboBoxGV.SelectedValue == null) return;

            bool ok = _db.InsertGiangDay(
                _maLop,
                comboBoxMH.SelectedValue.ToString(),
                comboBoxGV.SelectedValue.ToString());

            if (ok)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Thêm thất bại!");
            }
        }
    }
}
