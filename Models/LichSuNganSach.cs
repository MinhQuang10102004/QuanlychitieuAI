using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.Models
{
    public class LichSuNganSach
    {
        [Key]
        public int BudgetHistoryId { get; set; }
        public int BudgetId { get; set; }
        public DateTime Ngay { get; set; } = DateTime.UtcNow;
        public decimal SoTien { get; set; }    // giá trị cộng / trừ
        public string Loai { get; set; } = null!; // 'GiaoDich' | 'Manual' | 'Adjustment'
        public string? GhiChu { get; set; }

        public NganSach? Budget { get; set; }
    }
}
