using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChiTieu.Models
{
    public class GiaoDich
    {
        [Key]
        public int MaGiaoDich { get; set; }

        [ForeignKey(nameof(NguoiDung))]
        public int MaNguoiDung { get; set; }

        [ForeignKey(nameof(DanhMuc))]
        public int MaDanhMuc { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTien { get; set; }
        public DateTime NgayGiaoDich { get; set; }
        public string? MoTa { get; set; }

        // Nếu giao dịch là chung của hộ gia đình, gán MaHoGiaDinh
        public int? MaHoGiaDinh { get; set; }
        public bool IsShared { get; set; } = false;

        public NguoiDung? NguoiDung { get; set; }
        public DanhMuc? DanhMuc { get; set; }
    }
}
