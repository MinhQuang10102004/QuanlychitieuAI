namespace QuanLyChiTieu.Models
{
    public class TaiKhoanLichSu
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }
        public DateTime Ngay { get; set; } = DateTime.Now;
        public decimal SoTien { get; set; }
        public string? MoTa { get; set; }

        public TaiKhoanTaiChinh? TaiKhoan { get; set; }
    }
}
