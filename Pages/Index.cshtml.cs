using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using System.Linq;

namespace QuanLyChiTieu.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ChiTieuContext _context;

        public IndexModel(ChiTieuContext context)
        {
            _context = context;
        }

        public DateTime TargetMonth { get; private set; }
        public decimal TotalSpent { get; private set; }
        public int TransactionCount { get; private set; }
        public string TopCategory { get; private set; } = "Chưa có dữ liệu";
        public List<GiaoDich> LatestTransactions { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToPage("/Account/Login");
            }

            TargetMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            LatestTransactions = await _context.GiaoDiches
                .AsNoTracking()
                .OrderByDescending(g => g.NgayGiaoDich)
                .Include(g => g.DanhMuc)
                .Include(g => g.NguoiDung)
                .Take(10)
                .ToListAsync();

            TransactionCount = LatestTransactions.Count;
            TotalSpent = LatestTransactions.Sum(g => g.SoTien);

            var topCategory = LatestTransactions
                .GroupBy(g => g.DanhMuc?.TenDanhMuc ?? "Khác")
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            if (topCategory != null)
            {
                TopCategory = topCategory.Key;
            }

            return Page();
        }
    }
}
