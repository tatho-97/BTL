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

        // ==== TIỆN ÍCH CHUNG =========================================================
        private int Exec(string sql, params (string p, object v)[] prms)
        {
            try
            {
                Open();
                using var cmd = new SqliteCommand(sql, _conn);
                foreach (var t in prms) cmd.Parameters.AddWithValue(t.p, t.v ?? DBNull.Value);
                return cmd.ExecuteNonQuery();
            }
            finally { Close(); }
        }

        public bool ExistMaSV(string maSV)
        {
            const string sql = "SELECT 1 FROM SinhVien WHERE MaSV=@MaSV LIMIT 1;";
            return GetDataTable(sql, ("@MaSV", maSV)).Rows.Count > 0;
        }


        // ==== CRUD CHO SINH VIÊN ======================================================
        public bool InsertSinhVien(SinhVien sv)
        {
            const string sql = @"INSERT INTO SinhVien
        (MaSV, TenSV, NgaySinh, GioiTinh, DiaChi, MaLop)
        VALUES (@MaSV,@TenSV,@NgaySinh,@GioiTinh,@DiaChi,@MaLop);";

            return Exec(sql,
                ("@MaSV", sv.MaSV),
                ("@TenSV", sv.TenSV),
                ("@NgaySinh", sv.NgaySinh),
                ("@GioiTinh", sv.GioiTinh),
                ("@DiaChi", sv.DiaChi),
                ("@MaLop", sv.MaLop)) > 0;
        }

        public bool UpdateSinhVien(SinhVien sv)
        {
            const string sql = @"UPDATE SinhVien SET
        TenSV=@TenSV, NgaySinh=@NgaySinh, GioiTinh=@GioiTinh,
        DiaChi=@DiaChi, MaLop=@MaLop
        WHERE MaSV=@MaSV;";

            return Exec(sql,
                ("@MaSV", sv.MaSV),
                ("@TenSV", sv.TenSV),
                ("@NgaySinh", sv.NgaySinh),
                ("@GioiTinh", sv.GioiTinh),
                ("@DiaChi", sv.DiaChi),
                ("@MaLop", sv.MaLop)) > 0;
        }

        public bool DeleteSinhVien(string maSV)
        {
            // 1. Xoá điểm trước
            const string sqlDiem = "DELETE FROM Diem WHERE MaSV=@MaSV;";
            Exec(sqlDiem, ("@MaSV", maSV));

            // 2. Xoá sinh viên
            const string sqlSV = "DELETE FROM SinhVien WHERE MaSV=@MaSV;";
            return Exec(sqlSV, ("@MaSV", maSV)) > 0;
        }

        // ===== LẤY DỮ LIỆU ===========================================================
        public DataTable GetAllKhoa()
        {
            return GetDataTable("SELECT MaKhoa, TenKhoa FROM Khoa ORDER BY TenKhoa;");
        }

        public DataTable GetMonHocByKhoa(string maKhoa)
        {
            const string sql = @"SELECT MaMH, TenMH
                         FROM   MonHoc
                         WHERE  MaKhoa=@mk
                         ORDER  BY TenMH;";
            return GetDataTable(sql, ("@mk", maKhoa));
        }

        public DataTable GetAllGiangVien()
        {
            const string sql = @"
        SELECT gv.MaGV, gv.TenGV,
               k.TenKhoa, gv.MaKhoa,
               mh.TenMH,  gv.MaMH
        FROM   GiangVien gv
        LEFT JOIN Khoa    k  ON k.MaKhoa = gv.MaKhoa
        LEFT JOIN MonHoc  mh ON mh.MaMH  = gv.MaMH
        ORDER BY gv.MaGV;";
            return GetDataTable(sql);
        }

        public DataTable GetLopByGiangVien(string maGV)
        {
            const string sql = @"
        SELECT gd.MaLop, l.TenLop, gd.MaMH
        FROM   GiangDay gd
        JOIN   Lop l ON l.MaLop = gd.MaLop
        WHERE  gd.MaGV = @gv;";
            return GetDataTable(sql, ("@gv", maGV));
        }

        public bool ExistMaGV(string maGV) =>
            GetDataTable("SELECT 1 FROM GiangVien WHERE MaGV=@m LIMIT 1;",
                         ("@m", maGV)).Rows.Count > 0;

        // ===== CRUD GIẢNG VIÊN =======================================================
        public bool InsertGiangVien(GiangVien gv)
        {
            const string sql = @"INSERT INTO GiangVien
        (MaGV, TenGV, MaKhoa, MaMH)
        VALUES(@MaGV,@TenGV,@MaKhoa,@MaMH);";
            return Exec(sql,
                ("@MaGV", gv.MaGV),
                ("@TenGV", gv.TenGV),
                ("@MaKhoa", gv.MaKhoa),
                ("@MaMH", gv.MaMH)) > 0;
        }

        public bool UpdateGiangVien(GiangVien gv)
        {
            const string sql = @"UPDATE GiangVien SET
        TenGV=@TenGV, MaKhoa=@MaKhoa, MaMH=@MaMH
        WHERE MaGV=@MaGV;";
            return Exec(sql,
                ("@MaGV", gv.MaGV),
                ("@TenGV", gv.TenGV),
                ("@MaKhoa", gv.MaKhoa),
                ("@MaMH", gv.MaMH)) > 0;
        }

        public bool DeleteGiangVien(string maGV)
        {
            // Xoá bảng GiangDay trước để giữ toàn vẹn
            Exec("DELETE FROM GiangDay WHERE MaGV=@gv;", ("@gv", maGV));
            return Exec("DELETE FROM GiangVien WHERE MaGV=@gv;", ("@gv", maGV)) > 0;
        }
    }
}
