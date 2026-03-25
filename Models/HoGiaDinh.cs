using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.Models
{
    public class HoGiaDinh
    {
        [Key]
        public int MaHoGiaDinh { get; set; }
        public required string TenHoGiaDinh { get; set; }
        public int? ChuHoId { get; set; }
        public NguoiDung? ChuHo { get; set; }
        // Danh sách thành viên (không bắt buộc nhưng hữu ích để phân chia chi phí)
        public ICollection<NguoiDung> ThanhViens { get; set; } = new List<NguoiDung>();
    }
}
