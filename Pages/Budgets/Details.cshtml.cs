using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Pages.Budgets
{
    public class DetailsModel : PageModel
    {
        private readonly ChiTieuContext _context;

        public DetailsModel(ChiTieuContext context)
        {
            _context = context;
        }

        public NganSach? Budget { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Account/Login");

            Budget = await _context.Budgets.FirstOrDefaultAsync(b => b.BudgetId == id && b.MaNguoiDung == userId.Value);
            if (Budget == null) return NotFound();
            return Page();
        }
    }
}
