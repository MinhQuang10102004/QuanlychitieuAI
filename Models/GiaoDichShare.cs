using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.Models
{
    public class GiaoDichShare
    {
        [Key]
        public int Id { get; set; }
        public int GiaoDichId { get; set; }
        public int MaNguoiDung { get; set; }
        public decimal Amount { get; set; }
    }
}
