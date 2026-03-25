using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyChiTieu.Pages.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly ChiTieuContext _context;

        public IndexModel(ChiTieuContext context)
        {
            _context = context;
        }

        public IList<GiaoDich> Transactions { get; private set; } = new List<GiaoDich>();

        public async Task<IActionResult> OnGetAsync()
        {
            var redirect = RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            Transactions = await _context.GiaoDiches
                .AsNoTracking()
                .Include(g => g.NguoiDung)
                .Include(g => g.DanhMuc)
                .OrderByDescending(g => g.NgayGiaoDich)
                .ToListAsync();

            return Page();
        }

        private IActionResult? RequireLogin()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToPage("/Account/Login");
            }

            return null;
        }
    }
}
