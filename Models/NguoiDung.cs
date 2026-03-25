using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.Models
{
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }
        public required string HoTen { get; set; }
        public required string Email { get; set; }
        public required string MatKhau { get; set; }
        public string? Phone { get; set; }
        // AccountType: "Individual" or "Household"
        public string AccountType { get; set; } = "Individual";

        // Household membership
        public int? MaHoGiaDinh { get; set; }
        public HoGiaDinh? HoGiaDinh { get; set; }

        public ICollection<GiaoDich> GiaoDiches { get; set; } = new List<GiaoDich>();
    }
}