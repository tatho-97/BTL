using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTL
{
    class SinhVien
    {
        public string? MaSV;
        public string? TenSV;
        public string? NgaySinh;
        public string? GioiTinh;
        public string? DiaChi;
        public string? MaLop;
    }

    class Diem
    {
        public string? MaSV;
        public string? MaMH;
        public float DiemCC;
        public float DiemTX;
        public float DiemTHI;
        public float DiemHP;
    }

    class Lop
    {
        public string? MaLop;
        public string? TenLop;
    }

    class Khoa
    {
        public string? MaKhoa;
        public string? TenKhoa;
    }

    class MonHoc
    {
        public string? MaMH;
        public string? TenMH;
        public int TinChi;
        public string? MaKhoa;
    }

    class GiangVien
    {
        public string? MaGV;
        public string? TenGV;
        public string? MaKhoa;
        public string? MaMH;
    }

    class GiangDay
    {
        public string? MaGV;
        public string? MaMH;
        public string? MaLop;
    }

    class TaiKhoan
    {
        public string? Username;
        public string? Password;
        public string? Role;
        public string? MaGV;
        public string? MaKhoa;
    }
}
