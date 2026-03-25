using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly ChiTieuContext _context;

    public IndexModel(ChiTieuContext context)
    {
        _context = context;
    }

    public IList<DanhMuc> Categories { get; private set; } = new List<DanhMuc>();

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            return RedirectToPage("/Account/Login");
        }

        Categories = await _context.DanhMucs
            .AsNoTracking()
            .OrderBy(dm => dm.TenDanhMuc)
            .ToListAsync();

        return Page();
    }
}
