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
    public class Database
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

        // ===== LẤY DỮ LIỆU LỚP ======================================================
        public DataTable GetAllLop_Detail()
        {
            return GetDataTable("SELECT MaLop, TenLop FROM Lop ORDER BY TenLop;");
        }

        public DataTable GetSinhVienByLop(string maLop)
        {
            const string sql = @"
        SELECT MaSV, TenSV, GioiTinh, NgaySinh
        FROM   SinhVien
        WHERE  MaLop = @lop;";
            return GetDataTable(sql, ("@lop", maLop));
        }

        public DataTable GetMonByLop(string maLop)
        {
            const string sql = @"
        SELECT gd.MaMH, mh.TenMH, gd.MaGV, gv.TenGV
        FROM   GiangDay gd
        JOIN   MonHoc  mh ON mh.MaMH = gd.MaMH
        JOIN   GiangVien gv ON gv.MaGV = gd.MaGV
        WHERE  gd.MaLop = @lop;";
            return GetDataTable(sql, ("@lop", maLop));
        }

        // ===== CRUD LỚP ==============================================================
        // (cần PK MaLop duy nhất, còn MaKhoa là FK)
        public bool ExistMaLop(string maLop) =>
            GetDataTable("SELECT 1 FROM Lop WHERE MaLop=@m LIMIT 1;", ("@m", maLop)).Rows.Count > 0;

        public bool InsertLop(Lop lp) =>
    Exec("INSERT INTO Lop(MaLop, TenLop) VALUES(@Ma,@Ten);",
         ("@Ma", lp.MaLop), ("@Ten", lp.TenLop)) > 0;

        public bool UpdateLop(Lop lp) =>
            Exec("UPDATE Lop SET TenLop=@Ten WHERE MaLop=@Ma;",
                 ("@Ma", lp.MaLop), ("@Ten", lp.TenLop)) > 0;

        public bool DeleteLop(string maLop)
        {
            // 1. Xóa điểm của SV trong lớp
            Exec(@"DELETE FROM Diem
           WHERE MaSV IN (SELECT MaSV FROM SinhVien WHERE MaLop=@lop);",
                 ("@lop", maLop));

            // 2. Xóa sinh viên
            Exec("DELETE FROM SinhVien WHERE MaLop=@lop;", ("@lop", maLop));

            // 3. Xóa giảng dạy
            Exec("DELETE FROM GiangDay WHERE MaLop=@lop;", ("@lop", maLop));

            // 4. Xóa lớp
            return Exec("DELETE FROM Lop WHERE MaLop=@lop;", ("@lop", maLop)) > 0;
        }

        // ===== GIANGDAY (thêm môn cho lớp) ==========================================
        public bool InsertGiangDay(string maLop, string maMH, string maGV)
        {
            const string sql = "INSERT INTO GiangDay(MaGV, MaMH, MaLop) VALUES(@gv,@mh,@lop);";
            return Exec(sql, ("@gv", maGV), ("@mh", maMH), ("@lop", maLop)) > 0;
        }
        public bool DeleteGiangDay(string maLop, string maMH) =>
            Exec("DELETE FROM GiangDay WHERE MaLop=@lop AND MaMH=@mh;", ("@lop", maLop), ("@mh", maMH)) > 0;

        public DataTable GetGiangVienByMon(string maMH)
        {
            const string sql = "SELECT MaGV, TenGV FROM GiangVien WHERE MaMH=@mh;";
            return GetDataTable(sql, ("@mh", maMH));
        }

        public DataTable GetAllMonHoc_Detail()
        {
            const string sql =
                "SELECT mh.MaMH, mh.TenMH, mh.TinChi, mh.MaKhoa, k.TenKhoa " +
                "FROM   MonHoc mh LEFT JOIN Khoa k ON k.MaKhoa = mh.MaKhoa " +
                "ORDER BY mh.TenMH;";
            return GetDataTable(sql);
        }

        public bool ExistMaMH(string maMH) =>
            GetDataTable("SELECT 1 FROM MonHoc WHERE MaMH=@m LIMIT 1;", ("@m", maMH)).Rows.Count > 0;

        public bool InsertMonHoc(MonHoc mh)
        {
            const string sql = "INSERT INTO MonHoc(MaMH, TenMH, TinChi, MaKhoa) VALUES(@Ma,@Ten,@TC,@Khoa);";
            return Exec(sql,
                ("@Ma", mh.MaMH),
                ("@Ten", mh.TenMH),
                ("@TC", mh.TinChi),
                ("@Khoa", mh.MaKhoa)) > 0;
        }

        public bool UpdateMonHoc(MonHoc mh)
        {
            const string sql = "UPDATE MonHoc SET TenMH=@Ten, TinChi=@TC, MaKhoa=@Khoa WHERE MaMH=@Ma;";
            return Exec(sql,
                ("@Ma", mh.MaMH),
                ("@Ten", mh.TenMH),
                ("@TC", mh.TinChi),
                ("@Khoa", mh.MaKhoa)) > 0;
        }

        public bool DeleteMonHoc(string maMH)
        {
            // 1. Xoá điểm liên quan
            Exec("DELETE FROM Diem WHERE MaMH=@mh;", ("@mh", maMH));
            // 2. Xoá giảng dạy liên quan
            Exec("DELETE FROM GiangDay WHERE MaMH=@mh;", ("@mh", maMH));
            // 3. Bỏ liên kết môn cho giảng viên (set null)
            Exec("UPDATE GiangVien SET MaMH=NULL WHERE MaMH=@mh;", ("@mh", maMH));
            // 4. Xoá môn
            return Exec("DELETE FROM MonHoc WHERE MaMH=@mh;", ("@mh", maMH)) > 0;
        }

        public DataTable GetAllKhoa_Detail() =>
    GetDataTable("SELECT MaKhoa, TenKhoa FROM Khoa ORDER BY TenKhoa;");

        public bool ExistMaKhoa(string ma) =>
            GetDataTable("SELECT 1 FROM Khoa WHERE MaKhoa=@m LIMIT 1;", ("@m", ma)).Rows.Count > 0;

        public bool InsertKhoa(Khoa k) =>
            Exec("INSERT INTO Khoa(MaKhoa, TenKhoa) VALUES(@Ma,@Ten);",
                 ("@Ma", k.MaKhoa), ("@Ten", k.TenKhoa)) > 0;

        public bool UpdateKhoa(Khoa k) =>
            Exec("UPDATE Khoa SET TenKhoa=@Ten WHERE MaKhoa=@Ma;",
                 ("@Ma", k.MaKhoa), ("@Ten", k.TenKhoa)) > 0;

        public bool DeleteKhoa(string maKhoa)
        {
            // 1. Xoá điểm của tất cả môn thuộc khoa
            Exec(@"DELETE FROM Diem WHERE MaMH IN (SELECT MaMH FROM MonHoc WHERE MaKhoa=@k);",
                 ("@k", maKhoa));

            // 2. Xoá GiangDay liên quan môn & giảng viên của khoa
            Exec(@"DELETE FROM GiangDay WHERE MaMH IN (SELECT MaMH FROM MonHoc WHERE MaKhoa=@k);",
                 ("@k", maKhoa));
            Exec(@"DELETE FROM GiangDay WHERE MaGV IN (SELECT MaGV FROM GiangVien WHERE MaKhoa=@k);",
                 ("@k", maKhoa));

            // 3. Xoá GiangVien của khoa
            Exec("DELETE FROM GiangVien WHERE MaKhoa=@k;", ("@k", maKhoa));

            // 4. Xoá MonHoc của khoa
            Exec("DELETE FROM MonHoc WHERE MaKhoa=@k;", ("@k", maKhoa));

            // 5. Cuối cùng xoá Khoa
            return Exec("DELETE FROM Khoa WHERE MaKhoa=@k;", ("@k", maKhoa)) > 0;
        }

        public DataTable GetAllTaiKhoan()
        {
            const string sql = @"SELECT tk.Username, tk.Password, tk.Role,
                                 tk.MaGV, gv.TenGV
                          FROM   TaiKhoan tk
                          LEFT JOIN GiangVien gv ON gv.MaGV = tk.MaGV
                          ORDER  BY tk.Username;";
            return GetDataTable(sql);
        }

        public bool ExistUsername(string user) =>
            GetDataTable("SELECT 1 FROM TaiKhoan WHERE Username=@u LIMIT 1;", ("@u", user)).Rows.Count > 0;

        public bool InsertTaiKhoan(TaiKhoan tk)
        {
            const string sql = @"INSERT INTO TaiKhoan(Username,Password,Role,MaGV,MaKhoa)
                        VALUES(@u,@p,@r,@gv,@k);";
            return Exec(sql,
                ("@u", tk.Username),
                ("@p", tk.Password),   // ở bản demo để plain‑text
                ("@r", tk.Role),
                ("@gv", tk.MaGV),
                ("@k", tk.MaKhoa)) > 0;
        }

        public bool UpdatePassword(string user, string newPass)
        {
            return Exec("UPDATE TaiKhoan SET Password=@p WHERE Username=@u;",
                        ("@p", newPass), ("@u", user)) > 0;
        }

        public bool DeleteTaiKhoan(string user) =>
            Exec("DELETE FROM TaiKhoan WHERE Username=@u;", ("@u", user)) > 0;
    }
}
