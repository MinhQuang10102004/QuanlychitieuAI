namespace QuanLyChiTieu.Models
{
    public class TaiKhoanTaiChinh
    {
        public int Id { get; set; }
        public int MaNguoiDung { get; set; }
        public string TenTaiKhoan { get; set; } = null!;
        public string Loai { get; set; } = null!; // ViDienTu / NganHang / TienMat
        public decimal SoDu { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;

        public ICollection<TaiKhoanLichSu> LichSu { get; set; } = new List<TaiKhoanLichSu>();
    }
}
