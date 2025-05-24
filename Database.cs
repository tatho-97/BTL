using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTL
{
    internal class Database
    {
        // đặt file .db cạnh .exe (bin\Debug\…)
        private readonly string connectionString =
            $"Data Source={AppDomain.CurrentDomain.BaseDirectory}\\PMQLSVDH.db;";

        private SqliteConnection? _conn;

        private void Open()
        {
            _conn ??= new SqliteConnection(connectionString);
            if (_conn.State != ConnectionState.Open) _conn.Open();
        }
        private void Close()
        {
            if (_conn?.State == ConnectionState.Open) _conn.Close();
        }

        /// <summary>Trả về bản ghi TaiKhoan hợp lệ, null nếu sai.</summary>
        public TaiKhoan? CheckLogin(string username, string password)
        {
            try
            {
                Open();
                const string sql = @"SELECT * FROM TaiKhoan
                                     WHERE Username = @u AND Password = @p";
                using var cmd = new SqliteCommand(sql, _conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

                using var rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    return new TaiKhoan
                    {
                        Username = rd["Username"].ToString(),
                        Password = rd["Password"].ToString(),
                        Role = rd["Role"].ToString(),
                        MaGV = rd["MaGV"].ToString(),
                        MaKhoa = rd["MaKhoa"].ToString()
                    };
                }
                return null;
            }
            finally { Close(); }
        }

        public DataTable GetDataTable(string sql,
                                      params (string name, object value)[] prms)
            {
                var dt = new DataTable();
                try
                {
                    Open();
                    using var cmd = new SqliteCommand(sql, _conn);
                    foreach (var p in prms)
                        cmd.Parameters.AddWithValue(p.name, p.value ?? DBNull.Value);

                    using var reader = cmd.ExecuteReader();
                    dt.Load(reader);     // DataTable tự “hút” toàn bộ dữ liệu
                }
                finally { Close(); }

                return dt;
            }

            public DataTable GetAllSinhVien()
                {
                    const string sql = @"
                SELECT sv.MaSV, sv.TenSV, sv.NgaySinh,
                       sv.GioiTinh, sv.DiaChi,
                       l.TenLop AS LopHoc, sv.MaLop
                FROM   SinhVien sv
                LEFT JOIN Lop l ON l.MaLop = sv.MaLop
                ORDER BY sv.MaSV;";
                    return GetDataTable(sql);
                }

                // --- LỚP
                public DataTable GetAllLop()
                {
                    const string sql = "SELECT MaLop, TenLop FROM Lop ORDER BY TenLop;";
                    return GetDataTable(sql);
                }
            }
        }
