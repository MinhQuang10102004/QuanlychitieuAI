using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.Models
{
    public class DanhMuc
    {
        [Key]
        public int MaDanhMuc { get; set; }
        public required string TenDanhMuc { get; set; }

        public ICollection<GiaoDich> GiaoDiches { get; set; } = new List<GiaoDich>();
    }
}
