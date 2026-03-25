using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Pages.Users;

public class IndexModel : PageModel
{
    private readonly ChiTieuContext _context;

    public IndexModel(ChiTieuContext context)
    {
        _context = context;
    }

    public IList<UserSummary> Users { get; private set; } = new List<UserSummary>();

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            return RedirectToPage("/Account/Login");
        }

        Users = await _context.NguoiDungs
            .AsNoTracking()
            .GroupJoin(
                _context.GiaoDiches.AsNoTracking(),
                u => u.MaNguoiDung,
                g => g.MaNguoiDung,
                (u, giaoDiches) => new UserSummary
                {
                    HoTen = u.HoTen,
                    Email = u.Email,
                    TransactionCount = giaoDiches.Count()
                })
            .OrderBy(u => u.HoTen)
            .ToListAsync();

        return Page();
    }

    public class UserSummary
    {
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
    }
}
