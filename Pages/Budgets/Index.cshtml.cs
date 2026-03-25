using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Pages.Budgets
{
    public class IndexModel : PageModel
    {
        private readonly ChiTieuContext _context;

        public IndexModel(ChiTieuContext context)
        {
            _context = context;
        }

        public List<NganSach>? Budgets { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");

            Budgets = await _context.Budgets
                .Where(b => b.MaNguoiDung == userId.Value)
                .OrderByDescending(b => b.NgayTao)
                .ToListAsync();

            return Page();
        }
    }
}
