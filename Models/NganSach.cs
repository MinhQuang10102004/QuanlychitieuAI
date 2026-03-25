using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.Models
{
    public class NganSach
    {
        [Key]
        public int BudgetId { get; set; }
        public int MaNguoiDung { get; set; }
        public string TenBudget { get; set; } = null!;
        public decimal SoTien { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string TinhTrang { get; set; } = "Active";
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        public ICollection<LichSuNganSach> Histories { get; set; } = new List<LichSuNganSach>();
    }
}
