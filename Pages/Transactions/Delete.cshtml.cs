using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using System.Threading.Tasks;

namespace QuanLyChiTieu.Pages.Transactions
{
    public class DeleteModel : PageModel
    {
        private readonly ChiTieuContext _context;

        public DeleteModel(ChiTieuContext context)
        {
            _context = context;
        }

        public GiaoDich? Transaction { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var redirect = RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            Transaction = await _context.GiaoDiches
                .AsNoTracking()
                .Include(g => g.NguoiDung)
                .Include(g => g.DanhMuc)
                .FirstOrDefaultAsync(m => m.MaGiaoDich == id);

            if (Transaction == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var redirect = RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            var entity = await _context.GiaoDiches.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            _context.GiaoDiches.Remove(entity);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
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
