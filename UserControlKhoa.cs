﻿using System;
using System.Data;
using System.Windows.Forms;

namespace BTL
{
    public partial class UserControlKhoa : UserControl
    {
        private readonly Database _db = new();
        private DataTable _dt;
        private DataView _view;
        private enum Mode { View, Add, Edit }
        private Mode _mode = Mode.View;

        public UserControlKhoa()
        {
            InitializeComponent();
            Load += UserControlKhoa_Load;
        }

        private void UserControlKhoa_Load(object? sender, EventArgs e)
        {
            _dt = _db.GetAll<Khoa>();
            _view = _dt.DefaultView;
            dataGridView.DataSource = _view;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Đổi tiêu đề cột
            if (dataGridView.Columns.Contains("MaKhoa"))
                dataGridView.Columns["MaKhoa"].HeaderText = "Mã khoa";
            if (dataGridView.Columns.Contains("TenKhoa"))
                dataGridView.Columns["TenKhoa"].HeaderText = "Tên khoa";

            // Tùy chỉnh tỉ lệ fill của mỗi cột
            dataGridView.Columns["MaKhoa"].FillWeight = 30;
            dataGridView.Columns["TenKhoa"].FillWeight = 70;
            dataGridView.SelectionChanged += DataGridView_SelectionChanged;
            textBoxSearch.TextChanged += Search;

            buttonThem.Click += BtnThem_Click;
            buttonXoa.Click += BtnXoa_Click;
            buttonXN.Click += BtnXN_Click;
            buttonHuy.Click += BtnHuy_Click;

            if (dataGridView.Rows.Count > 0)
                DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        private void DataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            var row = ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row;
            textBox1.Text = row["MaKhoa"].ToString();
            textBox2.Text = row["TenKhoa"].ToString();
        }

        private void Search(object? sender, EventArgs e)
        {
            string kw = textBoxSearch.Text.Replace("'", "''").Trim();
            _view.RowFilter = string.IsNullOrEmpty(kw) ? "" :
                              $"MaKhoa LIKE '%{kw}%' OR TenKhoa LIKE '%{kw}%'";
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            _mode = Mode.Add;
            ToggleEdit(true);
            ClearInput();
        }

        private void BtnHuy_Click(object? sender, EventArgs e)
        {
            _mode = Mode.View;
            ToggleEdit(false);
            if (dataGridView.CurrentRow != null)
                DataGridView_SelectionChanged(null!, EventArgs.Empty);
        }

        private void BtnXN_Click(object? sender, EventArgs e)
        {
            var k = new Khoa { MaKhoa = textBox1.Text.Trim(), TenKhoa = textBox2.Text.Trim() };
            bool ok = false;
            if (_mode == Mode.Add)
            {
                if (_db.Exist<Khoa>(k.MaKhoa))
                {
                    MessageBox.Show("Mã khoa đã tồn tại");
                    return;
                }
                ok = _db.Insert<Khoa>(k);
            }
            else if (_mode == Mode.Edit)
            {
                ok = _db.Update<Khoa>(k);
            }
            MessageBox.Show(ok ? "Lưu thành công" : "Không thành công");
            BtnHuy_Click(null!, EventArgs.Empty);
            LoadData();
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;
            string ma = textBox1.Text;
            if (MessageBox.Show($"Xoá khoa {ma}?", "Xác nhận",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _db.Delete<Khoa>(ma);
                LoadData();
            }
        }

        private void ToggleEdit(bool enable)
        {
            dataGridView.Enabled = !enable;
            textBox1.Enabled = textBox2.Enabled = (_mode == Mode.Add);
            buttonXN.Visible = buttonHuy.Visible = enable;
            buttonThem.Visible = buttonXoa.Visible = !enable;
        }

        private void ClearInput()
        {
            textBox1.Clear();
            textBox2.Clear();
        }

        private void LoadData()
        {
            string filter = _view?.RowFilter ?? "";
            _dt = _db.GetAll<Khoa>();
            _view = new DataView(_dt) { RowFilter = filter };
            dataGridView.DataSource = _view;
        }
    }
}
